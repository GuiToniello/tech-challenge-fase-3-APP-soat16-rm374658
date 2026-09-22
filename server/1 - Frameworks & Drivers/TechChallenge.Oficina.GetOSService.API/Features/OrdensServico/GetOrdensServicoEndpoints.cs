using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

namespace TechChallenge.Oficina.GetOSService.API.Features.OrdensServico;

public static class GetOrdensServicoEndpoints
{
    public static void MapOrdensServicoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ordens-servico")
            .WithTags("OrdensServico");

        group.MapGet(
            "/ordenadas",
            (IOrdensServicoController ordensServicoController,
             CancellationToken cancellationToken) =>
                ordensServicoController.GetOrdenadas(cancellationToken))
            .Produces<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>(StatusCodes.Status200OK)
            .WithName("GetOrdensServicoOrdenadas")
            .WithDescription("Retorna lista de ordens de serviço ordenadas por prioridade e antiguidade, excluindo finalizadas/entregues.");
    }
}
