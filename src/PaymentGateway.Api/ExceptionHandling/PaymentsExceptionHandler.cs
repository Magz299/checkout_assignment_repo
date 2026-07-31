using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.ExceptionHandling;

public sealed class PaymentsExceptionHandler(
    ILogger<PaymentsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred while processing a payment request.");

        var (statusCode, title, detail) = exception switch
        {
            HttpRequestException { StatusCode: HttpStatusCode.ServiceUnavailable } =>
                (StatusCodes.Status503ServiceUnavailable,
                    "Acquiring bank unavailable",
                    "The payment could not be processed because the acquiring bank is unavailable."),
            HttpRequestException =>
                (StatusCodes.Status502BadGateway,
                    "Acquiring bank request failed",
                    "The payment could not be processed because the acquiring bank returned an unexpected response."),
            _ =>
                (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred",
                    "An unexpected error occurred while processing the request.")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            },
            cancellationToken);

        return true;
    }
}
