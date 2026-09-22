using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;

namespace TechChallenge.Oficina.ApprovalService.API.Features.OrdensServico
{
    public static class ApprovalServiceEndpoints
    {
        public static void MapEndpoints(RouteGroupBuilder group)
        {
            group.MapPost(
                "/aprovar/{id:guid}",
                (
                    IOrdensServicoController controller,
                    Guid id,
                    CancellationToken cancellationToken
                ) => AprovarOrcamento(controller, id, cancellationToken))
                .WithName("AprovarOrcamento")
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/recusar/{id:guid}",
                (
                    IOrdensServicoController controller,
                    Guid id,
                    CancellationToken cancellationToken
                ) => RecusarOrcamento(controller, id, cancellationToken))
                .WithName("RecusarOrcamento")
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<object> AprovarOrcamento(
            IOrdensServicoController controller,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await controller.AprovarOrcamento(id, cancellationToken);
        }

        private static async Task<object> RecusarOrcamento(
            IOrdensServicoController controller,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await controller.RecusarOrcamento(id, cancellationToken);
        }
    }
}
