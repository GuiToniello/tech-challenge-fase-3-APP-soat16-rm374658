using Moq;
using TechChallenge.Oficina.StatusService.API.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using Xunit;

namespace TechChallenge.Oficina.StatusService.API.Tests.Features.OrdensServico;

public sealed class StatusOrdemServicoEndpointsTests
{
    private readonly Mock<IClienteUseCases> _clienteUseCasesMock = new();
    private readonly Mock<IVeiculoUseCases> _veiculoUseCasesMock = new();
    private readonly Mock<IInsumoUseCases> _insumoUseCasesMock = new();
    private readonly Mock<IServicoUseCases> _servicoUseCasesMock = new();
    private readonly Mock<IOrdemServicoUseCases> _ordemServicoUseCasesMock = new();

    [Fact]
    public async Task MapStatusOrdemServicoEndpoints_GetAcompanhamento_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _ordemServicoUseCasesMock
            .Setup(service => service.ObterAcompanhamentoAsync(
                It.Is<ObterAcompanhamentoOrdemServicoPorIdQuery>(query => query.Id == id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcompanhamentoOrdemServicoViewModel { Id = id, Status = 1 });

        await controller.GetAcompanhamento(id, CancellationToken.None);

        _ordemServicoUseCasesMock.Verify(
            service => service.ObterAcompanhamentoAsync(
                It.Is<ObterAcompanhamentoOrdemServicoPorIdQuery>(query => query.Id == id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapStatusOrdemServicoEndpoints_GetAcompanhamento_DevePassarCancellationToken()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _ordemServicoUseCasesMock
            .Setup(service => service.ObterAcompanhamentoAsync(
                It.IsAny<ObterAcompanhamentoOrdemServicoPorIdQuery>(),
                cts.Token))
            .ReturnsAsync(new AcompanhamentoOrdemServicoViewModel { Id = id, Status = 1 });

        await controller.GetAcompanhamento(id, cts.Token);

        _ordemServicoUseCasesMock.Verify(
            service => service.ObterAcompanhamentoAsync(
                It.IsAny<ObterAcompanhamentoOrdemServicoPorIdQuery>(),
                cts.Token),
            Times.Once);
    }

    private IOrdensServicoController CriarController()
    {
        var adapter = new OrdensServicoAdapter();

        return new OrdensServicoController(
            _clienteUseCasesMock.Object,
            _veiculoUseCasesMock.Object,
            _insumoUseCasesMock.Object,
            _servicoUseCasesMock.Object,
            _ordemServicoUseCasesMock.Object,
            adapter);
    }
}
