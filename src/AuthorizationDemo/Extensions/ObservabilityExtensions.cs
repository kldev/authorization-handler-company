using AuthorizationDemo.Setup;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AuthorizationDemo.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservabilityAloy(this IServiceCollection services, IConfiguration config)
    {
        ObservabilityConfig observabilityConfig =
            config.GetSection(ObservabilityConfig.TAG_NAME).Get<ObservabilityConfig>() ?? new ObservabilityConfig();

        if (observabilityConfig.Enabled)
        {
            Console.WriteLine("---- Observability Aloy -----");
            Console.WriteLine($"URL: {observabilityConfig.ExporterUrl}");

            services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService("AuthorizationDemo"))
                .WithTracing(pb =>
                {
                    pb.AddSource("AuthorizationDemo")
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(observabilityConfig.ExporterUrl);
                        });
                })
                .WithLogging(pbx =>
                {
                    pbx.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(observabilityConfig.ExporterUrl);
                    }).AddConsoleExporter();

                }).WithMetrics(pbx =>
                {
                    pbx.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(observabilityConfig.ExporterUrl);
                        });
                });
        }

        return services;
    }

    public static WebApplicationBuilder SetupObservabilityLogging(this WebApplicationBuilder builder)
    {
        ObservabilityConfig observabilityConfig =
            builder.Configuration.GetSection(ObservabilityConfig.TAG_NAME).Get<ObservabilityConfig>() ?? new ObservabilityConfig();

        if (observabilityConfig.Enabled)
        {
            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
            });
        }

        // Always register Tracer so DI resolves even when observability is disabled.
        // TracerProvider.Default returns a no-op provider when OTel is not configured.
        builder.Services.AddSingleton(TracerProvider.Default.GetTracer("AuthorizationDemo", "1.0.0"));

        return builder;
    }
}