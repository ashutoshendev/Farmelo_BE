using Farmelo.Business.Commands.Auth;
using Farmelo.Business.Services.Security;
using Farmelo.Business.Support;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Entities;
using Farmelo.Data.Write.IRepository;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Auth;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, ServiceOperationResult<LoginResponseDto>>
{
    private readonly IDapperExecutor _dapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<UserAccount> _users;

    public RegisterCommandHandler(
        IDapperExecutor dapper,
        IPasswordHasher passwordHasher,
        IRepository<UserAccount> users)
    {
        _dapper = dapper;
        _passwordHasher = passwordHasher;
        _users = users;
    }

    public async Task<ServiceOperationResult<LoginResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Request.Email);
        var duplicate = await _dapper.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM UserAccounts WHERE Email = @Email",
            new { Email = normalizedEmail },
            ct: cancellationToken);

        if (duplicate > 0)
        {
            return ServiceOperationResult.CreateWithFailure<LoginResponseDto>("A user with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new UserAccount
        {
            FullName = request.Request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Request.Password),
            Role = AppConstants.Roles.User,
            IsActive = true,
            CreatedBy = "PUBLIC_SIGNUP",
            CreatedOn = now
        };

        await _users.AddAsync(user, cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(new LoginResponseDto
        {
            User = new AuthUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        }, "Account created.");
    }
}
