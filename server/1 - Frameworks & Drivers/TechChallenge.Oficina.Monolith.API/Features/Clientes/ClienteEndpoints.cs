using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;

namespace TechChallenge.Oficina.Monolith.API.Features.Clientes
{
    public static class ClienteEndpoints
    {
        public static RouteGroupBuilder MapClienteEndpoints(
             this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/clientes")
                .WithTags("Clientes");

            group.MapPost(
                string.Empty,
                (
                    IClienteController clienteController,
                    CriarClienteCommand command,
                    CancellationToken cancellationToken
                ) => clienteController.Post(command, cancellationToken))
                .WithName("PostCliente")
                .Produces<ClienteViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet(
                "/{id:guid}",
                (
                    IClienteController clienteController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => clienteController.GetById(id, cancellationToken))
                .WithName("GetClienteById")
                .Produces<ClienteViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    IClienteController clienteController,
                    CancellationToken cancellationToken
                ) => clienteController.Get(cancellationToken))
                .Produces<IReadOnlyCollection<ClienteViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    IClienteController clienteController,
                    AtualizarClienteCommand command,
                    CancellationToken cancellationToken
                ) => clienteController.Put(command, cancellationToken))
                .Produces<ClienteViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    IClienteController clienteController,
                    Guid id,
                    CancellationToken cancellationToken
                ) => clienteController.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
