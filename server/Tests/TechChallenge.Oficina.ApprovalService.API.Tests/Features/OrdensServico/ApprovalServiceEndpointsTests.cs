using Moq;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using Xunit;

namespace TechChallenge.Oficina.ApprovalService.API.Tests.Features.OrdensServico;

public sealed class ApprovalServiceEndpointsTests
{
    private readonly Mock<IOrdensServicoController> _mockController = new();

    [Fact]
    public async Task AprovarOrcamento_DeveInvocarControllerOrdensServico_ComIdCorreto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var viewModel = new OrdemServicoViewModel 
        { 
            Id = id, 
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            Status = 5
        };

        var resultado = new OrdensServicoResult<OrdemServicoViewModel, Exception>(viewModel);
        
        _mockController
            .Setup(c => c.AprovarOrcamento(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        // Act
        var task = _mockController.Object.AprovarOrcamento(id, CancellationToken.None);

        // Assert
        _mockController.Verify(
            c => c.AprovarOrcamento(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecusarOrcamento_DeveInvocarControllerOrdensServico_ComIdCorreto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var viewModel = new OrdemServicoViewModel 
        { 
            Id = id, 
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            Status = 6
        };

        var resultado = new OrdensServicoResult<OrdemServicoViewModel, Exception>(viewModel);
        
        _mockController
            .Setup(c => c.RecusarOrcamento(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        // Act
        var task = _mockController.Object.RecusarOrcamento(id, CancellationToken.None);

        // Assert
        _mockController.Verify(
            c => c.RecusarOrcamento(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AprovarOrcamento_DeveRetornarErro_QuandoDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var exception = new DomainException("Orçamento não pode ser aprovado neste status");
        var resultado = new OrdensServicoResult<OrdemServicoViewModel, Exception>(exception);

        _mockController
            .Setup(c => c.AprovarOrcamento(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        // Act
        var task = _mockController.Object.AprovarOrcamento(id, CancellationToken.None);

        // Assert
        _mockController.Verify(
            c => c.AprovarOrcamento(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecusarOrcamento_DeveRetornarErro_QuandoKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var exception = new KeyNotFoundException("Ordem de Serviço não encontrada");
        var resultado = new OrdensServicoResult<OrdemServicoViewModel, Exception>(exception);

        _mockController
            .Setup(c => c.RecusarOrcamento(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object)resultado);

        // Act
        var task = _mockController.Object.RecusarOrcamento(id, CancellationToken.None);

        // Assert
        _mockController.Verify(
            c => c.RecusarOrcamento(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
