using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;

namespace TechChallenge.Oficina.StatusService.API.Features.OrdensServico;

public static class StatusOrdemServicoEndpoints
{
    public static RouteGroupBuilder MapStatusOrdemServicoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/status/ordens-servico")
            .WithTags("StatusOrdemServico");

        group.MapGet(
            "/{id:guid}/acompanhamento",
            (
                IOrdensServicoController ordensServicoController,
                Guid id,
                CancellationToken cancellationToken
            ) => ordensServicoController.GetAcompanhamento(id, cancellationToken))
            .Produces<AcompanhamentoOrdemServicoViewModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
