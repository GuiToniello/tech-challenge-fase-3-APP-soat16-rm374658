using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;

namespace TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;

public interface IServicoUseCases
{
    Task<ServicoViewModel> CriarAsync(CriarServicoCommand command, CancellationToken cancellationToken = default);
    Task<ServicoViewModel> AtualizarAsync(AtualizarServicoCommand command, CancellationToken cancellationToken = default);
    Task<ServicoViewModel> ObterPorIdAsync(ObterServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServicoViewModel>> ListarAsync(ListarServicosQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirServicoCommand command, CancellationToken cancellationToken = default);
}
