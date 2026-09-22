using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;

namespace TechChallenge.Oficina.Controllers.Features.Insumos
{
    public interface IInsumoController
    {
        Task<object> Post(CriarInsumoCommand command, CancellationToken cancellationToken);

        Task<object> GetById(Guid id, CancellationToken cancellationToken);

        Task<object> Get(CancellationToken cancellationToken);

        Task<object> Put(AtualizarInsumoCommand command, CancellationToken cancellationToken);

        Task<object> Delete(Guid id, CancellationToken cancellationToken);
    }
}
