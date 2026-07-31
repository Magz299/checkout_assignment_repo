using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentGateway.Api.ExceptionHandling;

namespace PaymentGateway.Api.Tests.ExceptionHandling;

public class PaymentsExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Returns503_WhenTheBankIsUnavailable()
    {
        var handler = new PaymentsExceptionHandler(
            NullLogger<PaymentsExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new HttpRequestException("Bank unavailable", null, HttpStatusCode.ServiceUnavailable),
            CancellationToken.None);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Contains("Acquiring bank unavailable", body);
    }
}
