using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;

public interface IServicoGateway
{
    Task AdicionarAsync(Servico servico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default);
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Servico>> ListarAsync(CancellationToken cancellationToken = default);
    Task RemoverAsync(Servico servico, CancellationToken cancellationToken = default);
}
