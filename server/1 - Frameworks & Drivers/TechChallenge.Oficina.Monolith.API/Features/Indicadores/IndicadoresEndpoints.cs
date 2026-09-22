using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Indicadores;

namespace TechChallenge.Oficina.Monolith.API.Features.Indicadores
{
    public static class IndicadoresEndpoints
    {
        public static RouteGroupBuilder MapIndicadoresEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/indicadores")
                .WithTags("Indicadores");

            group.MapGet(
                string.Empty,
                (
                    IIndicadoresController controller,
                    CancellationToken cancellationToken
                ) => controller.Get(cancellationToken))
                .Produces<IndicadorViewModel>(StatusCodes.Status200OK);

            return group;
        }
    }
}
