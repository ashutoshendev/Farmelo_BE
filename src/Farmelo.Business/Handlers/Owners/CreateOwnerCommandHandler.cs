using Farmelo.Business.Commands.Owners;
using Farmelo.Business.Mapping;
using Farmelo.Business.Services.Security;
using Farmelo.Business.Support;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.Entities;
using Farmelo.Data.Write.IRepository;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Owners;

public sealed class CreateOwnerCommandHandler
    : IRequestHandler<CreateOwnerCommand, ServiceOperationResult<OwnerDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperExecutor _dapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<UserAccount> _users;

    public CreateOwnerCommandHandler(
        ICurrentUser currentUser,
        IDapperExecutor dapper,
        IPasswordHasher passwordHasher,
        IRepository<UserAccount> users)
    {
        _currentUser = currentUser;
        _dapper = dapper;
        _passwordHasher = passwordHasher;
        _users = users;
    }

    public async Task<ServiceOperationResult<OwnerDto>> Handle(
        CreateOwnerCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Request.Email);
        var duplicate = await _dapper.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM UserAccounts WHERE Email = @Email",
            new { Email = normalizedEmail },
            ct: cancellationToken);

        if (duplicate > 0)
        {
            return ServiceOperationResult.CreateWithFailure<OwnerDto>("A user with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var owner = new UserAccount
        {
            FullName = request.Request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Request.Password),
            Role = AppConstants.Roles.Owner,
            IsActive = true,
            CreatedBy = TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName),
            CreatedOn = now
        };

        await _users.AddAsync(owner, cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(OwnerDtoMapper.ToDto(owner), "Owner created.");
    }
}
