using Moq;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.Servicos;

public sealed class ServicosControllerTests
{
    private readonly Mock<IServicoUseCases> _serviceMock = new();
    private readonly Mock<IServicoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var servico = new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), true))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), true), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var controller = CriarController();
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoServicoExiste()
    {
        ConfigurarAdapterParaServicoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var servico = new ServicoViewModel { Id = id, Nome = "Troca" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<ServicoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(servico, resultado.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoServicoNaoExiste()
    {
        ConfigurarAdapterParaServicoResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<ServicoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        ConfigurarAdapterParaServicoResult();
        var controller = CriarController();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var servico = new ServicoViewModel { Id = command.Id, Nome = "Troca" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(servico);

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<ServicoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(servico, resultado.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        ConfigurarAdapterParaServicoResult();
        var controller = CriarController();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Troca", Descricao = "Descricao", ItensServico = [] };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<ServicoViewModel, Exception>>(response);

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<DomainException>(resultado.Error);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaServicoResult();
        var controller = CriarController();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Troca", Descricao = "Descricao", ItensServico = [] };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Put(command, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<ServicoViewModel, Exception>>(response);

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
        var resultado = Assert.IsType<ServicoResult<bool, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.True(resultado.Value);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        ConfigurarAdapterParaBoolResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var response = await controller.Delete(id, CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<bool, Exception>>(response);

        Assert.False(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<ServicoViewModel> servicos = new List<ServicoViewModel> { new() { Nome = "Troca" } };

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicos);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(servicos, resultado.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemServicos()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<ServicoViewModel> servicos = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicos);

        var response = await controller.Get(CancellationToken.None);
        var resultado = Assert.IsType<ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<ServicoViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private void ConfigurarAdapterParaServicoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<ServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns((ServicoResult<ServicoViewModel, Exception> result, bool _) => result);
    }

    private void ConfigurarAdapterParaColecaoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception>>()))
            .Returns((ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception> result) => result);
    }

    private void ConfigurarAdapterParaBoolResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ServicoResult<bool, Exception>>()))
            .Returns((ServicoResult<bool, Exception> result) => result);
    }

    private void ConfigurarAdapterParaEmpty()
    {
        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(new ServicoResult<bool, Exception>(true));
    }

    private ServicoController CriarController()
    {
        return new ServicoController(_serviceMock.Object, _adapterMock.Object);
    }
}
