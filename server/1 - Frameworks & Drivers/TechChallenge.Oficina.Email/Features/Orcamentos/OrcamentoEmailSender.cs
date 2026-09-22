using System.Globalization;
using System.Text;
using Resend;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

namespace TechChallenge.Oficina.Email.Features.Orcamentos;

public sealed class OrcamentoEmailSender : IOrcamentoEmailSender
{
    private readonly IResendClient _resendClient;
    private readonly ResendSettings _resendSettings;

    public OrcamentoEmailSender(IResendClient resendClient, ResendSettings resendSettings)
    {
        _resendClient = resendClient;
        _resendSettings = resendSettings;
    }

    public async Task EnviarOrcamentoAsync(OrdemServico ordemServico, string emailDestino, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_resendSettings.ApiKey) || string.IsNullOrWhiteSpace(_resendSettings.FromEmail))
        {
            throw new DomainException("Servico de email nao esta configurado. Verifique as configuracoes do Resend.");
        }

        if (ordemServico.Orcamento is null)
        {
            throw new DomainException("A ordem de servico informada nao possui orcamento gerado.");
        }

        var assunto = CriarAssunto(ordemServico);
        var html = CriarHtml(ordemServico);

        await _resendClient.SendEmailAsync(new EmailMessage
        {
            From = _resendSettings.FromEmail,
            To = emailDestino,
            Subject = assunto,
            HtmlBody = html
        }, cancellationToken);
    }

    private static string CriarAssunto(OrdemServico ordemServico)
    {
        var data = ordemServico.Orcamento!.DataGeracao.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var prefixo = ordemServico.Id.ToString()[..8].ToUpperInvariant();
        return $"Orçamento {prefixo} ({ordemServico.Id}) - data: {data}";
    }

    private static string CriarHtml(OrdemServico ordemServico)
    {
        var orcamento = ordemServico.Orcamento!;
        var cultura = new CultureInfo("pt-BR");
        var builder = new StringBuilder();

        builder.Append("<h1>Orçamento da Ordem de Serviço</h1>");
        builder.Append($"<p><strong>Ordem de Serviço:</strong> {ordemServico.Id}</p>");
        builder.Append($"<p><strong>Cliente:</strong> {ordemServico.ClienteId}</p>");
        builder.Append($"<p><strong>Veículo:</strong> {ordemServico.VeiculoId}</p>");
        builder.Append($"<p><strong>Status:</strong> {ordemServico.Status.ObterDescricao()}</p>");
        builder.Append($"<p><strong>Data de geração:</strong> {orcamento.DataGeracao.ToLocalTime():dd/MM/yyyy HH:mm}</p>");
        builder.Append($"<p><strong>Valor total:</strong> {orcamento.ValorTotal.ToString("C", cultura)}</p>");

        builder.Append("<h2>Serviços</h2>");
        builder.Append("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse;'>");
        builder.Append("<thead><tr><th>Serviço</th><th>Valor</th></tr></thead>");
        builder.Append("<tbody>");

        foreach (var servico in orcamento.Servicos)
        {
            builder.Append("<tr>");
            builder.Append($"<td>{servico.NomeServico} ({servico.ServicoId})</td>");
            builder.Append($"<td>{servico.ValorTotal.ToString("C", cultura)}</td>");
            builder.Append("</tr>");
        }

        builder.Append("</tbody></table>");

        return builder.ToString();
    }
}
