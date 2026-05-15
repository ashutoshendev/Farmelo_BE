using Farmelo.Data.Write.Entities;
using Farmelo.Shared.DTO.Owners;

namespace Farmelo.Business.Mapping;

internal static class OwnerDtoMapper
{
    public static OwnerDto ToDto(UserAccount owner)
        => new()
        {
            Id = owner.Id,
            FullName = owner.FullName,
            Email = owner.Email,
            IsActive = owner.IsActive,
            CreatedOn = owner.CreatedOn,
            ModifiedOn = owner.ModifiedOn
        };
}
