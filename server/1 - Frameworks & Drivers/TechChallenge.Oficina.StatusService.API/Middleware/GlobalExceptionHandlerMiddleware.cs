using System.Text.Json;
using TechChallenge.Oficina.DB.Data;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Logging;

namespace TechChallenge.Oficina.StatusService.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private static readonly EventId ErroNaoTratado = new(9001, "http.erro_nao_tratado");

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cliente desconectou: nao e erro do servidor, entao nao gera log de erro nem 5xx no APM
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception ex)
        {
            RegistrarErro(context, ex);
            await EscreverRespostaErroAsync(context);
        }
    }

    private void RegistrarErro(HttpContext context, Exception ex)
    {
        var routeTemplate = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;

        if (FalhasBanco.EhIndisponibilidade(ex))
        {
            _logger.LogError(ErroNaoTratado, ex, "Erro nao tratado em {RequestMethod} {RouteTemplate}: {Integracao} indisponivel",
                context.Request.Method, routeTemplate, OrdemServicoLogEventos.Integracoes.Postgres);
        }
        else
        {
            _logger.LogError(ErroNaoTratado, ex, "Erro nao tratado em {RequestMethod} {RouteTemplate}",
                context.Request.Method, routeTemplate);
        }
    }

    private static async Task EscreverRespostaErroAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var resposta = JsonSerializer.Serialize(new
        {
            message = "Ocorreu um erro interno. Tente novamente mais tarde."
        });

        await context.Response.WriteAsync(resposta);
    }
}
