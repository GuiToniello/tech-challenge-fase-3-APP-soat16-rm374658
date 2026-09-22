using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrdemServicoUseCases
{
    Task<OrdemServicoViewModel> CriarAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AtualizarAsync(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> ObterPorIdAsync(ObterOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServicoViewModel>> ListarAsync(ListarOrdensServicoQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>> ListarOrdenadasAsync(ListarOrdensServicoOrdenadasQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<AcompanhamentoOrdemServicoViewModel> ObterAcompanhamentoAsync(ObterAcompanhamentoOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>> ListarPorClienteAsync(ListarOrdensServicoPorClienteQuery query, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEmDiagnosticoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> GerarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task EnviarOrcamentoPorEmailAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AprovarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> RecusarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEmExecucaoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);    Task<OrdemServicoViewModel> AlterarStatusParaFinalizadaAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEntregueAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
