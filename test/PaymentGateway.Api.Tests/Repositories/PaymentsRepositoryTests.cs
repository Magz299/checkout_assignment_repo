using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Enums;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Tests.Repositories;

public class PaymentsRepositoryTests
{
    [Fact]
    public async Task GetAsync_ReturnsNull_WhenPaymentDoesNotExist()
    {
        var repository = new PaymentsRepository();

        var result = await repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetAsync_ReturnsThePaymentThatWasAdded()
    {
        var repository = new PaymentsRepository();
        var payment = new Payment
        {
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "4241",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "GBP",
            Amount = 1050
        };

        await repository.AddAsync(payment, CancellationToken.None);
        var result = await repository.GetAsync(payment.Id, CancellationToken.None);

        Assert.Same(payment, result);
    }
    
    [Fact]
    public async Task AddAsync_ThrowsInvalidOperationException_WhenPaymentWithSameIdAlreadyExists()
    {
        var repository = new PaymentsRepository();
        var payment = new Payment
        {
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "4241",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            Currency = "GBP",
            Amount = 1050
        };

        await repository.AddAsync(payment, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.AddAsync(payment, CancellationToken.None));
    }
}
