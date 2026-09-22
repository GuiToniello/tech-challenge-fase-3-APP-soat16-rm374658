using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrdemServicoUseCasesFacade
{
    IEstoqueUseCases EstoqueService { get; }
    IIndicadorUseCases IndicadorService { get; }
    IOrcamentoEmailSender OrcamentoEmailSender { get; }
    IOrdemServicoStatusEmailSender OrdemServicoStatusEmailSender { get; }
}
