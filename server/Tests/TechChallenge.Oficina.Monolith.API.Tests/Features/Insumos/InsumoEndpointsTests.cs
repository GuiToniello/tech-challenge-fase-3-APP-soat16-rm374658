using Moq;
using TechChallenge.Oficina.Adapters.Features.Insumos;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Monolith.API.Tests.Features.Insumos;

public sealed class InsumoEndpointsTests
{
    private readonly Mock<IInsumoUseCases> _insumoServiceMock = new();

    [Fact]
    public async Task MapInsumoEndpoints_Post_DeveInvocarServico_ComCommandCorreto()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var command = new CriarInsumoCommand { Nome = "Óleo Motor", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };
        var insumoViewModel = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Óleo Motor" };

        _insumoServiceMock
            .Setup(s => s.CriarAsync(It.Is<CriarInsumoCommand>(cmd => cmd.Nome == "Óleo Motor"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumoViewModel);

        await insumoController.Post(command, CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.CriarAsync(It.Is<CriarInsumoCommand>(cmd => cmd.Nome == "Óleo Motor"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_GetById_DeveInvocarServicoComIdCorreto()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var insumoId = Guid.NewGuid();
        var insumoViewModel = new InsumoViewModel { Id = insumoId, Nome = "Filtro" };

        _insumoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == insumoId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumoViewModel);

        await insumoController.GetById(insumoId, CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == insumoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_Get_DeveInvocarServico()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        IReadOnlyCollection<InsumoViewModel> insumos = [new InsumoViewModel { Nome = "Óleo" }];

        _insumoServiceMock
            .Setup(s => s.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumos);

        await insumoController.Get(CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_Put_DeveRepassarComandoInteiro()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var command = new AtualizarInsumoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro de Óleo",
            Fabricante = "Mann",
            QuantidadeDisponivel = 5,
            ValorUnitario = 30m
        };

        _insumoServiceMock
            .Setup(s => s.AtualizarAsync(It.IsAny<AtualizarInsumoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsumoViewModel());

        await insumoController.Put(command, CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.AtualizarAsync(
                It.Is<AtualizarInsumoCommand>(cmd =>
                    cmd.Nome == "Filtro de Óleo" &&
                    cmd.Fabricante == "Mann" &&
                    cmd.QuantidadeDisponivel == 5 &&
                    cmd.ValorUnitario == 30m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_Delete_DeveRepassarIdCorretamente()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var insumoId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _insumoServiceMock
            .Setup(s => s.ExcluirAsync(It.IsAny<ExcluirInsumoCommand>(), cts.Token))
            .Returns(Task.CompletedTask);

        await insumoController.Delete(insumoId, cts.Token);

        _insumoServiceMock.Verify(
            s => s.ExcluirAsync(
                It.Is<ExcluirInsumoCommand>(cmd => cmd.Id == insumoId),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_Post_DevePassarCancellationToken()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var command = new CriarInsumoCommand { Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 1, ValorUnitario = 10m };
        var cts = new CancellationTokenSource();

        _insumoServiceMock
            .Setup(s => s.CriarAsync(It.IsAny<CriarInsumoCommand>(), cts.Token))
            .ReturnsAsync(new InsumoViewModel());

        await insumoController.Post(command, cts.Token);

        _insumoServiceMock.Verify(
            s => s.CriarAsync(It.IsAny<CriarInsumoCommand>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_GetById_DeveRepassarIdExatamente()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var insumoId = Guid.NewGuid();

        _insumoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.IsAny<ObterInsumoPorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InsumoViewModel());

        await insumoController.GetById(insumoId, CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.ObterPorIdAsync(
                It.Is<ObterInsumoPorIdQuery>(q => q.Id == insumoId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapInsumoEndpoints_Delete_DevePassarCancellationToken()
    {
        var adapter = new InsumoAdapter();
        var insumoController = new Controllers.Features.Insumos.InsumoController(_insumoServiceMock.Object, adapter);
        var insumoId = Guid.NewGuid();

        _insumoServiceMock
            .Setup(s => s.ExcluirAsync(It.Is<ExcluirInsumoCommand>(cmd => cmd.Id == insumoId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await insumoController.Delete(insumoId, CancellationToken.None);

        _insumoServiceMock.Verify(
            s => s.ExcluirAsync(It.Is<ExcluirInsumoCommand>(cmd => cmd.Id == insumoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
