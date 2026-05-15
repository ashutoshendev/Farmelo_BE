using Farmelo.Shared.DTO.Business;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Business;

public sealed record CreatePartyCommand(PartyWriteRequestDto Request)
    : IRequest<ServiceOperationResult<PartyDto>>;

public sealed record UpdatePartyCommand(int PartyId, PartyWriteRequestDto Request)
    : IRequest<ServiceOperationResult<PartyDto>>;

public sealed record AddRawStockCommand(RawStockEntryCreateRequestDto Request)
    : IRequest<ServiceOperationResult<RawStockEntryDto>>;

public sealed record CreateBoxProductionCommand(BoxProductionCreateRequestDto Request)
    : IRequest<ServiceOperationResult<BoxStockSummaryDto>>;

public sealed record CreateB2BOrderCommand(B2BOrderCreateRequestDto Request)
    : IRequest<ServiceOperationResult<B2BOrderDto>>;

public sealed record CreateB2CAssignmentCommand(B2CAssignmentCreateRequestDto Request)
    : IRequest<ServiceOperationResult<B2CAssignmentDto>>;

public sealed record ReturnB2CStockCommand(B2CReturnRequestDto Request)
    : IRequest<ServiceOperationResult<B2CAssignmentDto>>;

public sealed record CreatePaymentCommand(PaymentCreateRequestDto Request)
    : IRequest<ServiceOperationResult<PaymentDto>>;
