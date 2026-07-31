namespace PaymentGateway.Api.Contracts.Responses;

public class BankResponse
{
    public bool Authorized { get; set; }

    public Guid AuthorizationCode { get; set; }
}