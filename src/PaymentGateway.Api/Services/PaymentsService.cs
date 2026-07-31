using FluentValidation;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Enums;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services;

public sealed class PaymentsService : IPaymentsService
{
    private readonly IBankClient _bankClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IValidator<PostPaymentRequest> _validator;

    public PaymentsService(
        IBankClient bankClient,
        IPaymentsRepository paymentsRepository,
        IValidator<PostPaymentRequest> validator)
    {
        _bankClient = bankClient;
        _paymentsRepository = paymentsRepository;
        _validator = validator;
    }

    public async Task<PaymentResponse> ProcessAsync(
        PostPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var rejectedPayment = new Payment
            {
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = GetLastFourDigits(request.CardNumber),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };
            
            return ToResponse(rejectedPayment);
        }

        var bankResponse = await _bankClient.ProcessAsync(
            new BankRequest
            {
                CardNumber = request.CardNumber,
                ExpiryDate = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1),
                Currency = request.Currency,
                Amount = request.Amount,
                Cvv = request.Cvv
            },
            cancellationToken);

        var payment = new Payment
        {
            Status = bankResponse.Authorized
                ? PaymentStatus.Authorized
                : PaymentStatus.Declined,
            CardNumberLastFour = GetLastFourDigits(request.CardNumber),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        await _paymentsRepository.AddAsync(payment, cancellationToken);

        return ToResponse(payment);
    }

    public async Task<PaymentResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentsRepository.GetAsync(id, cancellationToken);
        return payment is null ? null : ToResponse(payment);
    }

    private static string GetLastFourDigits(string? cardNumber)
    {
        if (cardNumber?.Length >= 4)
        {
            return cardNumber[^4..];
        }

        return cardNumber ?? string.Empty;
    }

    private static PaymentResponse ToResponse(Payment payment) => new()
    {
        Id = payment.Id,
        Status = payment.Status,
        CardNumberLastFour = payment.CardNumberLastFour,
        ExpiryMonth = payment.ExpiryMonth,
        ExpiryYear = payment.ExpiryYear,
        Currency = payment.Currency,
        Amount = payment.Amount
    };
}