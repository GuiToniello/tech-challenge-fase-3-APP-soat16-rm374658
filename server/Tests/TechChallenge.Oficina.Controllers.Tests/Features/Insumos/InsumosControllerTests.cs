using Moq;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.Insumos;

public sealed class InsumosControllerTests
{
    private readonly Mock<IInsumoUseCases> _serviceMock = new();
    private readonly Mock<IInsumoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarInsumoCommand { Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };
        var insumo = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Óleo" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumo);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<InsumoResult<InsumoViewModel, Exception>>(), true))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<InsumoResult<InsumoViewModel, Exception>>(), true), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarInsumoCommand { Nome = "", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<InsumoResult<InsumoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<InsumoResult<InsumoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoInsumoExiste()
    {
        ConfigurarAdapterParaInsumoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var insumo = new InsumoViewModel { Id = id, Nome = "Óleo" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumo);

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<InsumoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(insumo, resultado.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoInsumoNaoExiste()
    {
        ConfigurarAdapterParaInsumoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<InsumoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        ConfigurarAdapterParaInsumoResult();
        var controller = CriarController();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "Filtro", Fabricante = "Mann", QuantidadeDisponivel = 5, ValorUnitario = 30m };
        var insumo = new InsumoViewModel { Id = command.Id, Nome = "Filtro" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumo);

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<InsumoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(insumo, resultado.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        ConfigurarAdapterParaInsumoResult();
        var controller = CriarController();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "", Fabricante = "Mann", QuantidadeDisponivel = 5, ValorUnitario = 30m };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("nome inválido"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<InsumoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<DomainException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaInsumoResult();
        var controller = CriarController();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "Filtro", Fabricante = "Mann", QuantidadeDisponivel = 5, ValorUnitario = 30m };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<InsumoViewModel, Exception>>(response);

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

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirInsumoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<bool, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.True(resultado.Value);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirInsumoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaBoolResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirInsumoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<bool, Exception>>(response);

        Assert.False(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<InsumoViewModel> insumos = new List<InsumoViewModel> { new InsumoViewModel { Nome = "Óleo" } };

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumos);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(insumos, resultado.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemInsumos()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<InsumoViewModel> insumos = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumos);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<InsumoViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private void ConfigurarAdapterParaInsumoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<InsumoResult<InsumoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns((InsumoResult<InsumoViewModel, Exception> result, bool _) => result);
    }

    private void ConfigurarAdapterParaColecaoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception>>()))
            .Returns((InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception> result) => result);
    }

    private void ConfigurarAdapterParaBoolResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<InsumoResult<bool, Exception>>()))
            .Returns((InsumoResult<bool, Exception> result) => result);
    }

    private void ConfigurarAdapterParaEmpty()
    {
        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(new InsumoResult<bool, Exception>(true));
    }

    private InsumoController CriarController()
    {
        return new InsumoController(_serviceMock.Object, _adapterMock.Object);
    }
}
