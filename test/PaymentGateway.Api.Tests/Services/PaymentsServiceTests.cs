using FluentValidation;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain.Enums;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentsServiceTests
{
    [Theory]
    [InlineData(true, PaymentStatus.Authorized)]
    [InlineData(false, PaymentStatus.Declined)]
    public async Task ProcessAsync_StoresAndReturnsTheBankDecision_WhenRequestIsValid(
        bool isAuthorized,
        PaymentStatus expectedStatus)
    {
        var bankClient = new StubBankClient(isAuthorized);
        var repository = new PaymentsRepository();
        var validator = new StubValidator(isValid: true);
        var service = new PaymentsService(bankClient, repository, validator);
        var request = new PostPaymentRequest
        {
            CardNumber = "4242424242424241",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "GBP",
            Amount = 1050,
            Cvv = "123"
        };

        var response = await service.ProcessAsync(request, CancellationToken.None);
        var storedPayment = await repository.GetAsync(response.Id, CancellationToken.None);

        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal("4241", response.CardNumberLastFour);
        Assert.Equal(request.CardNumber, bankClient.LastRequest!.CardNumber);
        Assert.NotNull(storedPayment);
        Assert.Equal(expectedStatus, storedPayment!.Status);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsRejected_WhenValidationFails()
    {
        var bankClient = new StubBankClient(isAuthorized: true);
        var repository = new PaymentsRepository();
        var validator = new StubValidator(isValid: false);
        var service = new PaymentsService(bankClient, repository, validator);
        var request = new PostPaymentRequest
        {
            CardNumber = "123",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "GBP",
            Amount = 1050,
            Cvv = "123"
        };

        var response = await service.ProcessAsync(request, CancellationToken.None);

        Assert.Equal(PaymentStatus.Rejected, response.Status);
        Assert.Null(bankClient.LastRequest);
        Assert.Null(await repository.GetAsync(response.Id, CancellationToken.None));
    }

    private sealed class StubBankClient(bool isAuthorized) : IBankClient
    {
        public BankRequest? LastRequest { get; private set; }

        public Task<BankResponse> ProcessAsync(BankRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new BankResponse
            {
                Authorized = isAuthorized,
                AuthorizationCode = Guid.NewGuid()
            });
        }
    }

    private sealed class StubValidator : AbstractValidator<PostPaymentRequest>
    {
        public StubValidator(bool isValid)
        {
            if (!isValid)
            {
                RuleFor(x => x.CardNumber).Must(_ => false).WithMessage("Forced failure for test purposes.");
            }
        }
    }
}