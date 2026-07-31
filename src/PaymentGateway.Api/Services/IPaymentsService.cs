using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsService
{
    Task<PaymentResponse> ProcessAsync(PostPaymentRequest request, CancellationToken cancellationToken);
    
    Task<PaymentResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
}