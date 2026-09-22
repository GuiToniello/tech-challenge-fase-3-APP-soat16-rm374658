using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public interface IClienteController
    {
        Task<object> Post(CriarClienteCommand command, CancellationToken cancellationToken);

        Task<object> GetById(Guid id, CancellationToken cancellationToken);

        Task<object> Get(CancellationToken cancellationToken);

        Task<object> Put(AtualizarClienteCommand command, CancellationToken cancellationToken);

        Task<object> Delete(Guid id, CancellationToken cancellationToken);
    }
}
