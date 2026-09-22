using Moq;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.Clientes;

public sealed class ClientEndpointsTests
{
    private readonly Mock<IClienteUseCases> _serviceMock = new();
    private readonly Mock<IClienteAdapter> _adapterMock = new();


    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "Cliente" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>(), true))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>(), true), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "111" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoClienteExiste()
    {
        ConfigurarAdapterParaClienteResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var cliente = new ClienteViewModel { Id = id, NomeCompleto = "Cliente" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<ClienteViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(cliente, resultado.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoClienteNaoExiste()
    {
        ConfigurarAdapterParaClienteResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<ClienteViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        ConfigurarAdapterParaClienteResult();
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = command.Id, NomeCompleto = "Atualizado" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<ClienteViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(cliente, resultado.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        ConfigurarAdapterParaClienteResult();
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "111" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<ClienteViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<DomainException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaClienteResult();
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<ClienteViewModel, Exception>>(response);

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
        var resultado = Assert.IsType<ClienteResult<bool, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.True(resultado.Value);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaBoolResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<bool, Exception>>(response);

        Assert.False(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<ClienteViewModel> clientes = new List<ClienteViewModel> { new ClienteViewModel { NomeCompleto = "Cliente" } };

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(clientes, resultado.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemClientes()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<ClienteViewModel> clientes = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<ClienteViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private void ConfigurarAdapterParaClienteResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns((ClienteResult<ClienteViewModel, Exception> result, bool _) => result);
    }

    private void ConfigurarAdapterParaColecaoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception>>()))
            .Returns((ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception> result) => result);
    }

    private void ConfigurarAdapterParaBoolResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<bool, Exception>>()))
            .Returns((ClienteResult<bool, Exception> result) => result);
    }

    private void ConfigurarAdapterParaEmpty()
    {
        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(new ClienteResult<bool, Exception>(true));
    }

    private ClienteController CriarController()
    {
        return new ClienteController(_serviceMock.Object, _adapterMock.Object);
    }
}
