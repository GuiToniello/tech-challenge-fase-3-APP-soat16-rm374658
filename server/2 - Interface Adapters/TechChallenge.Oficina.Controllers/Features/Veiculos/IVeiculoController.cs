using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;

namespace TechChallenge.Oficina.Controllers.Features.Veiculos
{
    public interface IVeiculoController
    {
        Task<object> Post(CriarVeiculoCommand command, CancellationToken cancellationToken);

        Task<object> GetById(Guid id, CancellationToken cancellationToken);

        Task<object> Get(Guid? clienteId, CancellationToken cancellationToken);

        Task<object> Put(AtualizarVeiculoCommand command, CancellationToken cancellationToken);

        Task<object> Delete(Guid id, CancellationToken cancellationToken);
    }
}
