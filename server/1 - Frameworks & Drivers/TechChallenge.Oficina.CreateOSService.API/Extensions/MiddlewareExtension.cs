using TechChallenge.Oficina.CreateOSService.API.Middleware;

namespace TechChallenge.Oficina.CreateOSService.API.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
