using Serilog;
using Serilog.Events;
using Serilog.Templates;

namespace TechChallenge.Oficina.StatusService.API.Extensions;

public static class LoggingExtension
{
    private const string Template =
        "{ {..rest(), timestamp: UtcDateTime(@t), level: @l, message: @m, evento: EventId.Name, " +
        "logger: if IsDefined(SourceContext) then {name: SourceContext} else undefined(), " +
        "dd: if IsDefined(dd_service) then {trace_id: dd_trace_id, span_id: dd_span_id, service: dd_service, " +
        "env: dd_env, version: if dd_version <> '' then dd_version else undefined()} else undefined(), " +
        "error: if @x is not null then {kind: TypeOf(@x), message: Inspect(@x).Message, stack: ToString(@x)} else undefined()} }\n";

    public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(Template)));

        return services;
    }

    public static WebApplication UseLoggingConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null)
                {
                    return LogEventLevel.Error;
                }

                // /health* nao gera log de requisicao, nem o 503 do /health/ready: o HealthCheckService ja registra o Unhealthy como Error
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                {
                    return LogEventLevel.Verbose;
                }

                return httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var routeTemplate = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;

                if (routeTemplate is not null)
                {
                    diagnosticContext.Set("RouteTemplate", routeTemplate);
                }
            };
        });

        return app;
    }
}
