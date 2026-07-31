using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;

namespace PaymentGateway.Api.Clients;

public interface IBankClient
{
    Task<BankResponse> ProcessAsync(BankRequest request, CancellationToken cancellationToken);
}