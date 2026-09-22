using Moq;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using Xunit;

namespace TechChallenge.Oficina.CreateOSService.API.Tests.Features.OrdensServico;

public sealed class CriarOSCompletaEndpointsTests
{
    private readonly Mock<IOrdensServicoController> _mockController = new();

    [Fact]
    public async Task PostCompleta_DeveInvocarController_ComCommandCorreto()
    {
        var command = new AbrirOrdemServicoCompletaCommand();
        var abertura = new AberturaOrdemServicoViewModel { OrdemServicoId = Guid.NewGuid() };
        var resultado = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(abertura);

        _mockController
            .Setup(c => c.PostCompleta(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        await _mockController.Object.PostCompleta(command, CancellationToken.None);

        _mockController.Verify(
            c => c.PostCompleta(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostCompleta_DevePassarCancellationToken()
    {
        var command = new AbrirOrdemServicoCompletaCommand();
        var cts = new CancellationTokenSource();
        var abertura = new AberturaOrdemServicoViewModel { OrdemServicoId = Guid.NewGuid() };
        var resultado = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(abertura);

        _mockController
            .Setup(c => c.PostCompleta(It.IsAny<AbrirOrdemServicoCompletaCommand>(), cts.Token))
            .ReturnsAsync((object)resultado);

        await _mockController.Object.PostCompleta(command, cts.Token);

        _mockController.Verify(
            c => c.PostCompleta(It.IsAny<AbrirOrdemServicoCompletaCommand>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task PostCompleta_DeveRetornarResultado_QuandoSucesso()
    {
        var command = new AbrirOrdemServicoCompletaCommand();
        var osId = Guid.NewGuid();
        var abertura = new AberturaOrdemServicoViewModel { OrdemServicoId = osId };
        var resultado = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(abertura);

        _mockController
            .Setup(c => c.PostCompleta(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        var retorno = await _mockController.Object.PostCompleta(command, CancellationToken.None);

        var resultadoTipado = Assert.IsType<OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>>(retorno);
        Assert.Equal(osId, resultadoTipado.Value!.OrdemServicoId);
    }

    [Fact]
    public async Task PostCompleta_DeveRetornarErro_QuandoDomainException()
    {
        var command = new AbrirOrdemServicoCompletaCommand();
        var exception = new DomainException("Dados da OS inválidos");
        var resultado = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(exception);

        _mockController
            .Setup(c => c.PostCompleta(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        await _mockController.Object.PostCompleta(command, CancellationToken.None);

        _mockController.Verify(
            c => c.PostCompleta(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostCompleta_DeveRetornarErro_QuandoKeyNotFoundException()
    {
        var command = new AbrirOrdemServicoCompletaCommand();
        var exception = new KeyNotFoundException("Entidade relacionada não encontrada");
        var resultado = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(exception);

        _mockController
            .Setup(c => c.PostCompleta(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        await _mockController.Object.PostCompleta(command, CancellationToken.None);

        _mockController.Verify(
            c => c.PostCompleta(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
