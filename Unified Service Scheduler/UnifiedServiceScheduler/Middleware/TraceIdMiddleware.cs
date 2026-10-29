using System.Diagnostics;
using Serilog;
using Serilog.Context;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString()
                      ?? context.TraceIdentifier;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = traceId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("TraceId", traceId))
        {
            var logger = Log.ForContext<TraceIdMiddleware>();

            logger.Information(
                "HTTP request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.Error(ex,
                    "HTTP request failed: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }
            finally
            {
                logger.Information(
                    "HTTP request ended: {Method} {Path} with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
            }
        }
    }
}