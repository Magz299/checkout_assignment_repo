using System.Text.Json.Serialization;

public record BankAuthorizationResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; init; }

    [JsonPropertyName("authorization_code")]
    public Guid AuthorizationCode { get; init; }
}