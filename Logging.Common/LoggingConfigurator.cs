using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Logging.Common;

public static class LoggingConfigurator
{
    public static IHostApplicationBuilder ConfigureSerilog(
        this IHostApplicationBuilder builder,
        string serviceName)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(
                    new Uri(builder.Configuration["ElasticSettings:Url"]
                    ?? throw new("ElasticSettings:Url settings not found")))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"{serviceName.ToLower()}-logs-{environment.ToLower()}-{DateTime.UtcNow:yyyy.MM}"
                })
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder,
        string serviceName)
    {
        //builder.Services.AddOpenTelemetry()
        //    .WithTracing(tracer =>
        //    {
        //        tracer
        //            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        //            .AddAspNetCoreInstrumentation()
        //            .AddHttpClientInstrumentation()
        //            .AddEntityFrameworkCoreInstrumentation()
        //            .AddConsoleExporter()
        //            .AddJaegerExporter(o =>
        //            {
        //                o.AgentHost = "jaeger";
        //                o.AgentPort = 6831;
        //            });
        //    });

        return builder;
    }
}

