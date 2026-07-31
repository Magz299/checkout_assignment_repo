using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Validations;

namespace PaymentGateway.Api.Tests.Validations;

public class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValid_ForASupportedFuturePaymentRequest()
    {
        var result = _validator.Validate(ValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForInvalidCardCvvCurrencyAndAmount()
    {
        var request = ValidRequest();
        request.CardNumber = "not-card";
        request.Cvv = "12";
        request.Currency = "ABC";
        request.Amount = 0;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(PostPaymentRequest.CardNumber));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(PostPaymentRequest.Cvv));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(PostPaymentRequest.Currency));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(PostPaymentRequest.Amount));
    }

    private static PostPaymentRequest ValidRequest() => new()
    {
        CardNumber = "4242424242424241",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 1050,
        Cvv = "123"
    };
}
