using Microsoft.Extensions.Logging;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.Logging;

// Catálogo dos eventos de log de negócio da ordem de serviço. Nomes e valores são o contrato com as métricas e monitores do Datadog (docs/observabilidade.md e Terraform do repo K8S): não renomear sem atualizar os dois lados.
public static class OrdemServicoLogEventos
{
    public static readonly EventId OsCriada = new(1001, "os.criada");
    public static readonly EventId OsStatusAlterado = new(1002, "os.status_alterado");
    public static readonly EventId OsEncerrada = new(1003, "os.encerrada");
    public static readonly EventId OsOrcamentoGerado = new(1004, "os.orcamento_gerado");
    public static readonly EventId OsOrcamentoEnviado = new(1005, "os.orcamento_enviado");
    public static readonly EventId OsFalha = new(2001, "os.falha");
    public static readonly EventId IntegracaoFalha = new(3001, "integracao.falha");

    public static class Etapas
    {
        public const string Abertura = "abertura";
        public const string Atualizacao = "atualizacao";
        public const string Exclusao = "exclusao";
        public const string Diagnostico = "diagnostico";
        public const string OrcamentoGeracao = "orcamento_geracao";
        public const string OrcamentoEnvio = "orcamento_envio";
        public const string OrcamentoAprovacao = "orcamento_aprovacao";
        public const string OrcamentoRecusa = "orcamento_recusa";
        public const string Execucao = "execucao";
        public const string Finalizacao = "finalizacao";
        public const string Entrega = "entrega";
    }

    public static class TiposFalha
    {
        public const string RegraNegocio = "regra_negocio";
        public const string NaoEncontrado = "nao_encontrado";
    }

    public static class Integracoes
    {
        public const string Resend = "resend";
        public const string Postgres = "postgres";
    }

    public static class Operacoes
    {
        public const string EnvioOrcamento = "envio_orcamento";
        public const string NotificacaoStatus = "notificacao_status";
    }

    public static string StatusParaLog(StatusOrdemServico status)
    {
        return status switch
        {
            StatusOrdemServico.Recebida => "recebida",
            StatusOrdemServico.EmDiagnostico => "em_diagnostico",
            StatusOrdemServico.AguardandoAprovacao => "aguardando_aprovacao",
            StatusOrdemServico.EmExecucao => "em_execucao",
            StatusOrdemServico.Finalizada => "finalizada",
            StatusOrdemServico.Entregue => "entregue",
            StatusOrdemServico.Encerrada => "encerrada",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Status da ordem de servico invalido.")
        };
    }
}
