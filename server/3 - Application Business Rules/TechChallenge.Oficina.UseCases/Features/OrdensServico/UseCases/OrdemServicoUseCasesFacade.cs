using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

// Este facade agrupa serviços auxiliares relacionados ao fluxo de ordem de serviço para reduzir a quantidade de dependências injetadas no OrdemServicoService e atender à regra S107 do SonarQube sem alterar o padrão arquitetural atual.
public sealed class OrdemServicoUseCasesFacade : IOrdemServicoUseCasesFacade
{
    public OrdemServicoUseCasesFacade(
        IEstoqueUseCases estoqueService,
        IIndicadorUseCases indicadorService,
        IOrcamentoEmailSender orcamentoEmailSender,
        IOrdemServicoStatusEmailSender ordemServicoStatusEmailSender)
    {
        EstoqueService = estoqueService;
        IndicadorService = indicadorService;
        OrcamentoEmailSender = orcamentoEmailSender;
        OrdemServicoStatusEmailSender = ordemServicoStatusEmailSender;
    }

    public IEstoqueUseCases EstoqueService { get; }

    public IIndicadorUseCases IndicadorService { get; }

    public IOrcamentoEmailSender OrcamentoEmailSender { get; }

    public IOrdemServicoStatusEmailSender OrdemServicoStatusEmailSender { get; }
}
