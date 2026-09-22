using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

public interface IInsumoGateway
{
    Task AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default);
    Task<Insumo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Insumo>> ListarAsync(CancellationToken cancellationToken = default);
    Task RemoverAsync(Insumo insumo, CancellationToken cancellationToken = default);
}
