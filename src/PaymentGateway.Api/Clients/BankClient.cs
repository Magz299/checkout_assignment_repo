using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;

namespace PaymentGateway.Api.Clients;

public sealed class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;

    public BankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankResponse> ProcessAsync(
        BankRequest request,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            card_number = request.CardNumber,
            expiry_date = request.ExpiryDate.ToString("MM/yyyy", CultureInfo.InvariantCulture),
            currency = request.Currency,
            amount = request.Amount,
            cvv = request.Cvv
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "payments", payload, cancellationToken);

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new HttpRequestException(
                "The acquiring bank is unavailable.",
                inner: null,
                statusCode: response.StatusCode);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BankResponse>(
                   cancellationToken: cancellationToken)
               ?? throw new HttpRequestException("The acquiring bank returned an empty response.");
    }
}
