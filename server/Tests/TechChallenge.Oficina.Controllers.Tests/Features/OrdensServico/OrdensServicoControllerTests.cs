using Moq;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerTests
{
    private readonly Mock<IClienteUseCases> _clienteUseCasesMock = new();
    private readonly Mock<IVeiculoUseCases> _veiculoUseCasesMock = new();
    private readonly Mock<IInsumoUseCases> _insumoUseCasesMock = new();
    private readonly Mock<IServicoUseCases> _servicoUseCasesMock = new();
    private readonly Mock<IOrdemServicoUseCases> _serviceMock = new();
    private readonly Mock<IOrdensServicoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var ordem = new OrdemServicoViewModel { Id = Guid.NewGuid() };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task PostCompleta_DeveOrquestrarFluxoERetornarAdaptado_QuandoSucesso()
    {
        var controller = CriarController();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var insumoId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var adaptedResult = new object();

        var command = new AbrirOrdemServicoCompletaCommand
        {
            Cliente = new ClienteAberturaOrdemServicoCommand { NomeCompleto = "Cliente Completo", Identificacao = "529.982.247-25", Email = "cliente@oficina.com" },
            Veiculo = new VeiculoAberturaOrdemServicoCommand { Placa = "ABC1D23", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901" },
            Servicos =
            [
                new ServicoAberturaOrdemServicoCommand
                {
                    Nome = "Troca de Oleo",
                    Descricao = "Troca completa",
                    ItensServico =
                    [
                        new ItemServicoAberturaOrdemServicoCommand
                        {
                            Insumo = new InsumoAberturaOrdemServicoCommand { Nome = "Oleo 5W30", Fabricante = "Mobil", QuantidadeDisponivel = 20, ValorUnitario = 30m },
                            Quantidade = 4
                        }
                    ]
                }
            ]
        };

        _clienteUseCasesMock
            .Setup(service => service.CriarAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClienteViewModel { Id = clienteId });

        _veiculoUseCasesMock
            .Setup(service => service.CriarAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VeiculoViewModel { Id = veiculoId });

        _insumoUseCasesMock
            .Setup(service => service.CriarAsync(It.IsAny<CriarInsumoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsumoViewModel { Id = insumoId });

        _servicoUseCasesMock
            .Setup(service => service.CriarAsync(It.IsAny<CriarServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServicoViewModel { Id = servicoId });

        _serviceMock
            .Setup(service => service.CriarAsync(
                It.Is<CriarOrdemServicoCommand>(c => c.ClienteId == clienteId && c.VeiculoId == veiculoId && c.ServicoIds.Count == 1 && c.ServicoIds.Contains(servicoId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrdemServicoViewModel { Id = ordemServicoId });

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var response = await controller.PostCompleta(command, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _clienteUseCasesMock.Verify(service => service.CriarAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _veiculoUseCasesMock.Verify(service => service.CriarAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _insumoUseCasesMock.Verify(service => service.CriarAsync(It.IsAny<CriarInsumoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _servicoUseCasesMock.Verify(service => service.CriarAsync(It.IsAny<CriarServicoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(service => service.CriarAsync(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var ordem = new OrdemServicoViewModel { Id = id };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.GetById(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_DeveRepassarQuery()
    {
        var controller = CriarController();
        IReadOnlyCollection<OrdemServicoViewModel> ordens = [new OrdemServicoViewModel { Id = Guid.NewGuid() }];
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordens);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.Get(CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(adaptedResult);

        var response = await controller.Delete(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GerarOrcamento_DeveRepassarComandoComId()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrdemServicoViewModel { Id = id });

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.GerarOrcamento(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(adaptedResult);

        var response = await controller.EnviarOrcamento(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de dominio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var response = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    private OrdensServicoController CriarController()
    {
        return new OrdensServicoController(
            _clienteUseCasesMock.Object,
            _veiculoUseCasesMock.Object,
            _insumoUseCasesMock.Object,
            _servicoUseCasesMock.Object,
            _serviceMock.Object,
            _adapterMock.Object);
    }
}
