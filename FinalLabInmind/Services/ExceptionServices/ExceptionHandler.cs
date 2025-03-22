using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace FinalLabInmind.Services.ExceptionServices;

public class ExceptionHandler: IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ArgumentException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            case UnauthorizedAccessException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;
            default:
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var response = new { message = exception.Message };
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response), cancellationToken);
        return true;
    }
}