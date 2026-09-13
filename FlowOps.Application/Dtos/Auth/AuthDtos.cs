namespace FlowOps.Application.Dtos.Auth;

public class RegisterDto
{
    public string OrganizationName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class TokenResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
