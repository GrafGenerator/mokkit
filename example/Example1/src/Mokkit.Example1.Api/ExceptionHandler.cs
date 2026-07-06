using Microsoft.AspNetCore.Diagnostics;

namespace Mokkit.Example1.Api;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            new ExceptionResult(StatusCodes.Status400BadRequest, "Error occurred: " + exception.Message),
            cancellationToken: cancellationToken);

        return true;
    }

    public record ExceptionResult(int Code, string Message);
}