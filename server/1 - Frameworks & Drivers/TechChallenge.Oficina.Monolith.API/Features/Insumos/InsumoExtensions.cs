using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Insumos;

namespace TechChallenge.Oficina.Monolith.API.Features.Insumos
{
    public static class InsumoExtensions
    {
        public static RouteGroupBuilder MapInsumoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/insumos")
                .WithTags("Insumos");

            group.MapPost(
                string.Empty,
                (
                    IInsumoController insumoController,
                    CriarInsumoCommand command,
                    CancellationToken cancellationToken
                ) => insumoController.Post(command, cancellationToken))
                .Produces<InsumoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet(
                "/{id:guid}",
                (
                    IInsumoController insumoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => insumoController.GetById(id, cancellationToken))
                .WithName("GetInsumoById")
                .Produces<InsumoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    IInsumoController insumoController,
                    CancellationToken cancellationToken
                ) => insumoController.Get(cancellationToken))
                .Produces<IReadOnlyCollection<InsumoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    IInsumoController insumoController,
                    AtualizarInsumoCommand command,
                    CancellationToken cancellationToken
                ) => insumoController.Put(command, cancellationToken))
                .Produces<InsumoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    IInsumoController insumoController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => insumoController.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
