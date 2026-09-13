using System.Threading;
using System.Threading.Tasks;
using FlowOps.Application.Dtos.Auth;
using FlowOps.Domain.Entities;

namespace FlowOps.Application.Common.Interfaces;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
