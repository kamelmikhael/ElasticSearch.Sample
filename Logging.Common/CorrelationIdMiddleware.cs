using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;

namespace Logging.Common;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.ContainsKey(CorrelationHeader)
            ? context.Request.Headers[CorrelationHeader].ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[CorrelationHeader] = correlationId;
        LogContext.PushProperty("CorrelationId", correlationId);

        // Attach trace ID if available
        var activity = Activity.Current;
        if (activity != null)
        {
            LogContext.PushProperty("TraceId", activity.TraceId.ToString());
            LogContext.PushProperty("SpanId", activity.SpanId.ToString());
        }

        await _next(context);
    }
}

