using System.Collections.Concurrent;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Repositories;

public sealed class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (!_payments.TryAdd(payment.Id, payment))
        {
            throw new InvalidOperationException($"A payment with id '{payment.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Payment?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        _payments.TryGetValue(id, out var payment);
        return Task.FromResult(payment);
    }
}
