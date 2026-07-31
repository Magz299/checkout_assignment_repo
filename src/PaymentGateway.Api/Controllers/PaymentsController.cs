using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain.Enums;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> ProcessPaymentAsync(
        [FromBody] PostPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentsService.ProcessAsync(request, cancellationToken);

        if (payment.Status == PaymentStatus.Rejected)
        {
            return BadRequest(payment);
        }

        return CreatedAtRoute(
            "GetPayment",
            new { id = payment.Id },
            payment);
    }

    [HttpGet("{id:guid}", Name = "GetPayment")]
    public async Task<ActionResult<PaymentResponse>> GetPaymentAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentsService.GetAsync(id, cancellationToken);

        return payment is null ? NotFound() : Ok(payment);
    }
}
