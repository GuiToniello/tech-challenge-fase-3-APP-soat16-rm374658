using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;

namespace TechChallenge.Oficina.CreateOSService.API.Features.OrdensServico;

public static class CriarOSCompletaEndpoints
{
    public static RouteGroupBuilder MapCriarOSCompletaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/ordens-servico")
            .WithTags("OrdensServico");

        group.MapPost(
            "/completa",
            (
                IOrdensServicoController ordensServicoController,
                AbrirOrdemServicoCompletaCommand command,
                CancellationToken cancellationToken
            ) => ordensServicoController.PostCompleta(command, cancellationToken))
            .Produces<AberturaOrdemServicoViewModel>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet(
            "/{id:guid}",
            (
                IOrdensServicoController ordensServicoController,
                Guid id,
                CancellationToken cancellationToken
            ) => ordensServicoController.GetById(id, cancellationToken))
            .WithName("GetOrdemServicoById")
            .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
