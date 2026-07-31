namespace PaymentGateway.Api.Contracts.Requests;

public class BankRequest
{
    public string CardNumber { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }
}