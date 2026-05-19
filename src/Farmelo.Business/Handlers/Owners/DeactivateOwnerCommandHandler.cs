using Farmelo.Business.Commands.Owners;
using Farmelo.Business.Mapping;
using Farmelo.Business.Support;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.Entities;
using Farmelo.Data.Write.IRepository;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Owners;

public sealed class DeactivateOwnerCommandHandler
    : IRequestHandler<DeactivateOwnerCommand, ServiceOperationResult<OwnerDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<UserAccount> _users;

    public DeactivateOwnerCommandHandler(ICurrentUser currentUser, IRepository<UserAccount> users)
    {
        _currentUser = currentUser;
        _users = users;
    }

    public async Task<ServiceOperationResult<OwnerDto>> Handle(
        DeactivateOwnerCommand request,
        CancellationToken cancellationToken)
    {
        var owner = await _users.FindAsync(request.OwnerId, cancellationToken);
        if (owner == null || owner.Role != AppConstants.Roles.Owner)
        {
            return ServiceOperationResult.CreateWithFailure<OwnerDto>("Owner was not found.");
        }

        owner.IsActive = false;
        owner.ModifiedBy = TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName);
        owner.ModifiedOn = DateTime.UtcNow;

        await _users.UpdateAsync(owner, cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(OwnerDtoMapper.ToDto(owner), "Owner deactivated.");
    }
}
