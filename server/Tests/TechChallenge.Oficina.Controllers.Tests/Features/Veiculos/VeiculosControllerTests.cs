using Moq;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.Veiculos;

public sealed class VeiculosControllerTests
{
    private readonly Mock<IVeiculoUseCases> _serviceMock = new();
    private readonly Mock<IVeiculoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand
        {
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var veiculo = new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), true))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), true), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand
        {
            Placa = "INVALIDA",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand
        {
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoVeiculoExiste()
    {
        ConfigurarAdapterParaVeiculoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var veiculo = new VeiculoViewModel { Id = id, Placa = "ABC1D23" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<VeiculoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(veiculo, resultado.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoVeiculoNaoExiste()
    {
        ConfigurarAdapterParaVeiculoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<VeiculoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        ConfigurarAdapterParaVeiculoResult();
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand
        {
            Id = Guid.NewGuid(),
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var veiculo = new VeiculoViewModel { Id = command.Id, Placa = "ABC1D23" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<VeiculoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(veiculo, resultado.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        ConfigurarAdapterParaVeiculoResult();
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand
        {
            Id = Guid.NewGuid(),
            Placa = "INVALIDA",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<VeiculoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<DomainException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaVeiculoResult();
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand
        {
            Id = Guid.NewGuid(),
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<VeiculoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        ConfigurarAdapterParaEmpty();
        var controller = CriarController();
        var id = Guid.NewGuid();

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<bool, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.True(resultado.Value);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaBoolResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<bool, Exception>>(response);

        Assert.False(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<VeiculoViewModel> veiculos = new List<VeiculoViewModel>
        {
            new() { Id = Guid.NewGuid(), Placa = "ABC1D23" }
        };

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        var response = await controller.Get(null, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<IReadOnlyCollection<VeiculoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(veiculos, resultado.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemVeiculos()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<VeiculoViewModel> veiculos = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        var response = await controller.Get(null, CancellationToken.None);
        var resultado = Assert.IsType<VeiculoResult<IReadOnlyCollection<VeiculoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<VeiculoViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private void ConfigurarAdapterParaVeiculoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<VeiculoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns((VeiculoResult<VeiculoViewModel, Exception> result, bool _) => result);
    }

    private void ConfigurarAdapterParaColecaoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<IReadOnlyCollection<VeiculoViewModel>, Exception>>()))
            .Returns((VeiculoResult<IReadOnlyCollection<VeiculoViewModel>, Exception> result) => result);
    }

    private void ConfigurarAdapterParaBoolResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<VeiculoResult<bool, Exception>>()))
            .Returns((VeiculoResult<bool, Exception> result) => result);
    }

    private void ConfigurarAdapterParaEmpty()
    {
        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(new VeiculoResult<bool, Exception>(true));
    }

    private VeiculoController CriarController()
    {
        return new VeiculoController(_serviceMock.Object, _adapterMock.Object);
    }
}
