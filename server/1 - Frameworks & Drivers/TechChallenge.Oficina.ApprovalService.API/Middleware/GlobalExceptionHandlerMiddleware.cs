using System.Text.Json;

namespace TechChallenge.Oficina.ApprovalService.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excecao nao tratada capturada pelo middleware global.");
            await EscreverRespostaErroAsync(context);
        }
    }

    private static async Task EscreverRespostaErroAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var resposta = JsonSerializer.Serialize(new
        {
            message = "Ocorreu um erro interno. Tente novamente mais tarde."
        });

        await context.Response.WriteAsync(resposta);
    }
}
