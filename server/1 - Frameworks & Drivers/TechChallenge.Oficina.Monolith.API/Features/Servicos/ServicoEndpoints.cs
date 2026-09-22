using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Servicos;

namespace TechChallenge.Oficina.Monolith.API.Features.Servicos
{
    public static class ServicoEndpoints
    {
        public static RouteGroupBuilder MapServicoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/servicos")
                .WithTags("Servicos");

            group.MapPost(
                string.Empty,
                (
                    IServicoController servicoController,
                    CriarServicoCommand command,
                    CancellationToken cancellationToken
                ) => servicoController.Post(command, cancellationToken))
                .Produces<ServicoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/{id:guid}",
                (
                    IServicoController servicoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => servicoController.GetById(id, cancellationToken))
                .WithName("GetServicoById")
                .Produces<ServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    IServicoController servicoController,
                    CancellationToken cancellationToken
                ) => servicoController.Get(cancellationToken))
                .Produces<IReadOnlyCollection<ServicoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    IServicoController servicoController,
                    AtualizarServicoCommand command,
                    CancellationToken cancellationToken
                ) => servicoController.Put(command, cancellationToken))
                .Produces<ServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    IServicoController servicoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => servicoController.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
