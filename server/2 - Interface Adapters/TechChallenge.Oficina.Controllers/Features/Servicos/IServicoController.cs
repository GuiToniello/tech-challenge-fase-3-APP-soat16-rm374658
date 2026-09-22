using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;

namespace TechChallenge.Oficina.Controllers.Features.Servicos
{
    public interface IServicoController
    {
        Task<object> Post(CriarServicoCommand command, CancellationToken cancellationToken);

        Task<object> GetById(Guid id, CancellationToken cancellationToken);

        Task<object> Get(CancellationToken cancellationToken);

        Task<object> Put(AtualizarServicoCommand command, CancellationToken cancellationToken);

        Task<object> Delete(Guid id, CancellationToken cancellationToken);
    }
}
