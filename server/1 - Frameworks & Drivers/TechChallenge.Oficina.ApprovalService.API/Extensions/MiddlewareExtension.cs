using TechChallenge.Oficina.ApprovalService.API.Middleware;

namespace TechChallenge.Oficina.ApprovalService.API.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
