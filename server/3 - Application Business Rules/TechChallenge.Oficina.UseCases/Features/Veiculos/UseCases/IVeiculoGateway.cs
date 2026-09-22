using TechChallenge.Oficina.Entities.Features.Veiculos;

namespace TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

public interface IVeiculoGateway
{
    Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Veiculo>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<bool> ExisteComPlacaAsync(string placa, Guid? ignorarVeiculoId = null, CancellationToken cancellationToken = default);
    Task RemoverAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
}
