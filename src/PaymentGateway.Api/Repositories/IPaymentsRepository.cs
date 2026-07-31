using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Repositories;

public interface IPaymentsRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task<Payment?> GetAsync(Guid id, CancellationToken cancellationToken);
}