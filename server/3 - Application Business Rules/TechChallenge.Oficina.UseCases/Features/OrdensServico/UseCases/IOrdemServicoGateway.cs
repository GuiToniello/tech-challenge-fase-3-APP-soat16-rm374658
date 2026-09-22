using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrdemServicoGateway
{
    Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarOrdenadasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarPorStatusAsync(StatusOrdemServico status, CancellationToken cancellationToken = default);
    Task RemoverAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
}
