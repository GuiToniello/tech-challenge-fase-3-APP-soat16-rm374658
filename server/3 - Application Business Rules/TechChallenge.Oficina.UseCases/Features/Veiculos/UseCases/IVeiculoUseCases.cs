using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;

namespace TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

public interface IVeiculoUseCases
{
    Task<VeiculoViewModel> CriarAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default);
    Task<VeiculoViewModel> AtualizarAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default);
    Task<VeiculoViewModel> ObterPorIdAsync(ObterVeiculoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VeiculoViewModel>> ListarAsync(ListarVeiculosQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default);
}
