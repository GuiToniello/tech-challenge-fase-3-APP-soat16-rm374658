namespace TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

public static class StatusOrdemServicoExtensions
{
    public static string ObterDescricao(this StatusOrdemServico status)
    {
        return status switch
        {
            StatusOrdemServico.Recebida => "Recebida",
            StatusOrdemServico.EmDiagnostico => "Em diagnóstico",
            StatusOrdemServico.AguardandoAprovacao => "Aguardando aprovação",
            StatusOrdemServico.EmExecucao => "Em execução",
            StatusOrdemServico.Finalizada => "Finalizada",
            StatusOrdemServico.Entregue => "Entregue",
            StatusOrdemServico.Encerrada => "Encerrada",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status da ordem de servico invalido.")
        };
    }
}
