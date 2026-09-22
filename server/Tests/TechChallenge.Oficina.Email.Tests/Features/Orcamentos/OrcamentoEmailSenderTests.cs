using Moq;
using Resend;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Email.Configuration;
using TechChallenge.Oficina.Email.Features.Orcamentos;
using Xunit;
using TechChallenge.Oficina.Email.Features;

namespace TechChallenge.Oficina.DB.Email.Tests.Features.Orcamentos;

public sealed class OrcamentoEmailSenderTests
{
    private readonly Mock<IResendClient> _resendClientMock = new();

    [Fact]
    public async Task EnviarOrcamentoAsync_DeveMontarEmailEEnviar()
    {
        var ordemServico = CriarOrdemServicoComOrcamento();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev" };
        var sender = new OrcamentoEmailSender(_resendClientMock.Object, settings);
        EmailMessage? emailCapturado = null;

        _resendClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((message, _) => emailCapturado = message)
            .Returns(Task.CompletedTask);

        await sender.EnviarOrcamentoAsync(ordemServico, "cliente@teste.com");

        _resendClientMock.Verify(client => client.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(emailCapturado);
        Assert.Contains("onboarding@resend.dev", emailCapturado!.From.ToString());
        Assert.NotNull(emailCapturado.To);
        Assert.Contains(ordemServico.Id.ToString(), emailCapturado.Subject);
        Assert.Contains("data:", emailCapturado.Subject);
        Assert.Contains(ordemServico.Id.ToString(), emailCapturado.HtmlBody);
        Assert.Contains("Valor total", emailCapturado.HtmlBody);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_DeveLancarQuandoNaoHouverOrcamento()
    {
        var servico = CriarServico("Revisao", 1, 50m);
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = "onboarding@resend.dev" };
        var sender = new OrcamentoEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarOrcamentoAsync(ordemServico, "cliente@teste.com");

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("A ordem de servico informada nao possui orcamento gerado.", exception.Message);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_DeveLancarDomainExceptionQuandoApiKeyNaoConfigurada()
    {
        var ordemServico = CriarOrdemServicoComOrcamento();
        var settings = new ResendSettings { ApiKey = string.Empty, FromEmail = "onboarding@resend.dev" };
        var sender = new OrcamentoEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarOrcamentoAsync(ordemServico, "cliente@teste.com");

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Servico de email nao esta configurado. Verifique as configuracoes do Resend.", exception.Message);
    }

    [Fact]
    public async Task EnviarOrcamentoAsync_DeveLancarDomainExceptionQuandoFromEmailNaoConfigurado()
    {
        var ordemServico = CriarOrdemServicoComOrcamento();
        var settings = new ResendSettings { ApiKey = "api-key", FromEmail = string.Empty };
        var sender = new OrcamentoEmailSender(_resendClientMock.Object, settings);

        var action = async () => await sender.EnviarOrcamentoAsync(ordemServico, "cliente@teste.com");

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Servico de email nao esta configurado. Verifique as configuracoes do Resend.", exception.Message);
    }

    private static OrdemServico CriarOrdemServicoComOrcamento()
    {
        var servico = CriarServico("Troca de oleo", 2, 25m);
        var ordemServico = OrdemServico.Criar(Guid.NewGuid(), Guid.NewGuid(), [servico]);
        ordemServico.GerarOrcamento(new DateTime(2026, 06, 06, 10, 00, 00, DateTimeKind.Utc));
        return ordemServico;
    }

    private static Servico CriarServico(string nome, int quantidade, decimal valorUnitario)
    {
        var insumo = Insumo.Criar("Insumo", "Fabricante", 10, valorUnitario);
        return Servico.Criar(nome, "Descricao", [ItemServico.Criar(insumo, quantidade)]);
    }
}
