using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Common.Interfaces;
using FlowOps.Application.Dtos.Auth;
using FlowOps.Domain.Entities;
using FlowOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlowOps.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly FlowOpsDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IConfiguration _configuration;

    public AuthService(
        FlowOpsDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IDateTimeProvider dateTimeProvider,
        IConfiguration configuration)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
        _configuration = configuration;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new Exception("Email already in use."); // In production, use a custom domain exception
        }

        // 1. Create Tenant (Organization)
        var org = new Organization
        {
            Name = request.OrganizationName,
            Slug = request.OrganizationName.ToLower().Replace(" ", "-")
        };
        _context.Organizations.Add(org);

        // 2. Create Admin Role and Permissions (simplified for Phase 4)
        var adminRole = new Role { Name = "Admin", Description = "Root Administrator", TenantId = org.Id };
        _context.Roles.Add(adminRole);

        // 3. Create User
        var user = new User
        {
            TenantId = org.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
            Organization = org
        };

        user.UserRoles.Add(new UserRole { User = user, Role = adminRole });
        _context.Users.Add(user);

        await _context.SaveChangesAsync(cancellationToken);

        // 4. Generate Tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(expiryDays),
            UserId = user.Id,
            TenantId = org.Id
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password.");
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(expiryDays),
            UserId = user.Id,
            TenantId = user.TenantId
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAtUtc <= _dateTimeProvider.UtcNow)
        {
            return null;
        }

        // Revoke the old token
        tokenEntity.IsRevoked = true;

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(tokenEntity.User);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        tokenEntity.ReplacedByToken = newRefreshToken;

        var newTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresAtUtc = _dateTimeProvider.UtcNow.AddDays(expiryDays),
            UserId = tokenEntity.UserId,
            TenantId = tokenEntity.TenantId
        };

        _context.RefreshTokens.Add(newTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
    }
}
