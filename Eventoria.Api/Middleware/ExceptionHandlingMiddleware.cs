using System.Net;
using System.Text.Json;

namespace Eventoria.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        var (status, code) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "forbidden"),
            ArgumentException or ArgumentNullException or ArgumentOutOfRangeException
                or InvalidOperationException => (HttpStatusCode.BadRequest, "bad_request"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "not_found"),
            _ => (HttpStatusCode.InternalServerError, "server_error")
        };

        var payload = new ApiErrorResponse(
            ErrorCode: code,
            Message: ex.Message,
            TraceId: traceId
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private sealed record ApiErrorResponse(string ErrorCode, string Message, string TraceId);
}
