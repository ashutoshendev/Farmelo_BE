using Farmelo.Business.Commands.Auth;
using Farmelo.Business.Services.Security;
using Farmelo.Business.Support;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Auth;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, ServiceOperationResult<LoginResponseDto>>
{
    private readonly IDapperExecutor _dapper;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IDapperExecutor dapper, IPasswordHasher passwordHasher)
    {
        _dapper = dapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceOperationResult<LoginResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Request.Email);
        var user = await _dapper.QuerySingleAsync<UserAccountLoginRecord>(
            """
            SELECT Id, FullName, Email, PasswordHash, Role, IsActive
            FROM UserAccounts
            WHERE Email = @Email
            """,
            new { Email = normalizedEmail },
            ct: cancellationToken);

        if (user == null || !user.IsActive || !_passwordHasher.VerifyPassword(request.Request.Password, user.PasswordHash))
        {
            return ServiceOperationResult.CreateWithFailure<LoginResponseDto>("Invalid email or password.");
        }

        return ServiceOperationResult.CreateWithSuccess(new LoginResponseDto
        {
            User = new AuthUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        });
    }

    private sealed class UserAccountLoginRecord
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
