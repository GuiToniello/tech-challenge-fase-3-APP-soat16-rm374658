using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Veiculos;

namespace TechChallenge.Oficina.Monolith.API.Features.Veiculos
{
    public static class VeiculoEndpoints
    {
        public static RouteGroupBuilder MapVeiculoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/veiculos")
                .WithTags("Veiculos");

            group.MapPost(
                string.Empty,
                (
                    IVeiculoController veiculoController,
                    CriarVeiculoCommand command,
                    CancellationToken cancellationToken
                ) => veiculoController.Post(command, cancellationToken))
                .Produces<VeiculoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/{id:guid}",
                (
                    IVeiculoController veiculoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => veiculoController.GetById(id, cancellationToken))
                .WithName("GetVeiculoById")
                .Produces<VeiculoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    IVeiculoController veiculoController,
                    Guid? clienteId,
                    CancellationToken cancellationToken
                ) => veiculoController.Get(clienteId, cancellationToken))
                .Produces<IReadOnlyCollection<VeiculoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    IVeiculoController veiculoController,
                    AtualizarVeiculoCommand command,
                    CancellationToken cancellationToken
                ) => veiculoController.Put(command, cancellationToken))
                .Produces<VeiculoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    IVeiculoController veiculoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => veiculoController.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
