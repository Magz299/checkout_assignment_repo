using FluentValidation;
using PaymentGateway.Api.Contracts.Requests;

namespace PaymentGateway.Api.Validations;

public sealed class PaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly string[] SupportedCurrencies = ["GBP", "USD", "EUR"];

    public PaymentRequestValidator()
    {
        RuleFor(request => request.CardNumber)
            .NotEmpty()
            .Matches("^[0-9]{14,19}$")
            .WithMessage("Card number must contain 14 to 19 digits.");

        RuleFor(request => request.ExpiryMonth)
            .InclusiveBetween(1, 12);

        RuleFor(request => request.ExpiryYear)
            .GreaterThan(0);

        RuleFor(request => request)
            .Must(HaveFutureExpiry)
            .WithMessage("Expiry month and year must be in the future.");

        RuleFor(request => request.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(3)
            .Must(currency => SupportedCurrencies.Contains(currency.ToUpperInvariant()))
            .WithMessage("Currency must be one of GBP, USD, or EUR.");

        RuleFor(request => request.Amount)
            .GreaterThan(0);

        RuleFor(request => request.Cvv)
            .NotEmpty()
            .Matches("^[0-9]{3,4}$")
            .WithMessage("CVV must contain 3 or 4 digits.");
    }

    private static bool HaveFutureExpiry(PostPaymentRequest request)
    {
        if (request.ExpiryMonth is < 1 or > 12 || request.ExpiryYear is < 1 or > 9999)
        {
            return false;
        }

        var expiryMonth = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1);
        var currentMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        return expiryMonth > currentMonth;
    }
}
