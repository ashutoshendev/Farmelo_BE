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

public sealed class UpdateOwnerCommandHandler
    : IRequestHandler<UpdateOwnerCommand, ServiceOperationResult<OwnerDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperExecutor _dapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<UserAccount> _users;

    public UpdateOwnerCommandHandler(
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
        UpdateOwnerCommand request,
        CancellationToken cancellationToken)
    {
        var owner = await _users.FindAsync(request.OwnerId, cancellationToken);
        if (owner == null || owner.Role != AppConstants.Roles.Owner)
        {
            return ServiceOperationResult.CreateWithFailure<OwnerDto>("Owner was not found.");
        }

        var normalizedEmail = TextNormalizer.NormalizeEmail(request.Request.Email);
        var duplicate = await _dapper.QuerySingleAsync<int>(
            """
            SELECT COUNT(1)
            FROM UserAccounts
            WHERE Id <> @OwnerId AND Email = @Email
            """,
            new { request.OwnerId, Email = normalizedEmail },
            ct: cancellationToken);

        if (duplicate > 0)
        {
            return ServiceOperationResult.CreateWithFailure<OwnerDto>("A user with this email already exists.");
        }

        owner.FullName = request.Request.FullName.Trim();
        owner.Email = normalizedEmail;
        owner.IsActive = request.Request.IsActive;
        owner.ModifiedBy = TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName);
        owner.ModifiedOn = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Request.Password))
        {
            owner.PasswordHash = _passwordHasher.HashPassword(request.Request.Password);
        }

        await _users.UpdateAsync(owner, cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(OwnerDtoMapper.ToDto(owner), "Owner updated.");
    }
}
