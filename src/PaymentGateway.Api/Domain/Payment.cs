using PaymentGateway.Api.Domain.Enums;

namespace PaymentGateway.Api.Domain;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}