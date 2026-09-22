using Moq;
using Resend;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.Email.Features.OrdensServico;
using Xunit;
using TechChallenge.Oficina.Email.Features;

namespace TechChallenge.Oficina.Email.Tests.Features.OrdensServico;

public sealed class OrdemServicoStatusEmailSenderTests
{
    private readonly Mock<IResendClient> _resendClientMock = new();

    [Theory]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public async Task EnviarStatusAlteradoAsync_DeveEnviarEmailComAssuntoCustomizadoParaStatus(StatusOrdemServico status)
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarStatusAlteradoAsync(ordemServico, "cliente@teste.com", status);

        _resendClientMock.Verify(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(emailCapturado);
        Assert.Contains("onboarding@resend.dev", emailCapturado!.From.ToString());
        Assert.NotNull(emailCapturado.To);
        Assert.Contains("cliente@teste.com", emailCapturado.To);
        Assert.Contains(ordemServico.Id.ToString()[..8].ToUpperInvariant(), emailCapturado.Subject);
        Assert.Contains("Status", emailCapturado.Subject);
        Assert.Contains(ordemServico.Id.ToString(), emailCapturado.HtmlBody);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveRetornarSemErroQuandoApiKeyNaoConfigurada()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = string.Empty, FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.EmDiagnostico);

        // Não deve lançar exceção em modo degradado
        await action();

        _resendClientMock.Verify(
            client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveRetornarSemErroQuandoFromEmailNaoConfigurado()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = string.Empty, SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.EmExecucao);

        // Não deve lançar exceção em modo degradado
        await action();

        _resendClientMock.Verify(
            client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveRetornarSemErroQuandoSendEmailOnStatusChangeEstaDesabilitado()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = false };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.EmDiagnostico);

        // Não deve lançar exceção mesmo com flag desabilitada
        await action();

        _resendClientMock.Verify(
            client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveContarAssinaturaPrefixoCorretamenteNoAssunto()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.EmDiagnostico);

        Assert.NotNull(emailCapturado);
        var prefixoEsperado = ordemServico.Id.ToString()[..8].ToUpperInvariant();
        Assert.Contains(prefixoEsperado, emailCapturado!.Subject);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveConterDadosOrdemServicoNoHTML()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.Finalizada);

        Assert.NotNull(emailCapturado);
        Assert.Contains(ordemServico.Id.ToString(), emailCapturado!.HtmlBody);
        Assert.Contains(ordemServico.ClienteId.ToString(), emailCapturado.HtmlBody);
        Assert.Contains(ordemServico.VeiculoId.ToString(), emailCapturado.HtmlBody);
        Assert.Contains("finalizada", emailCapturado.HtmlBody, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnviarStatusAlteradoAsync_DeveConterServicosDaOSNoHTML()
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarStatusAlteradoAsync(
            ordemServico,
            "cliente@teste.com",
            StatusOrdemServico.EmExecucao);

        Assert.NotNull(emailCapturado);
        foreach (var servico in ordemServico.Servicos)
        {
            Assert.Contains(servico.Nome, emailCapturado!.HtmlBody);
        }
    }

    [Theory]
    [InlineData(StatusOrdemServico.EmDiagnostico, "diagnóstico")]
    [InlineData(StatusOrdemServico.EmExecucao, "execução")]
    [InlineData(StatusOrdemServico.Finalizada, "finalizada")]
    [InlineData(StatusOrdemServico.Entregue, "entregue")]
    public async Task EnviarStatusAlteradoAsync_DeveConterDetalhesEspecificosPorStatus(StatusOrdemServico status, string textoEsperado)
    {
        var ordemServico = CriarOrdemServico();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev", SendEmailOnStatusChange = true };
        var sender = new OrdemServicoStatusEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarStatusAlteradoAsync(ordemServico, "cliente@teste.com", status);

        Assert.NotNull(emailCapturado);
        Assert.Contains(textoEsperado, emailCapturado!.HtmlBody, System.StringComparison.OrdinalIgnoreCase);
    }

    private static OrdemServico CriarOrdemServico()
    {
        var servico = CriarServico("Revisao", 1, 50m);
        return OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);
    }

    private static Servico CriarServico(string nome, int ordem, decimal valor)
    {
        return Servico.Criar(nome, "Descricao do servico", null);
    }
}
