using Moq;
using TechChallenge.Oficina.Adapters.Features.Veiculos;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using Xunit;

namespace TechChallenge.Oficina.Monolith.API.Tests.Features.Veiculos;

public sealed class VeiculoEndpointsTests
{
    private readonly Mock<IVeiculoUseCases> _veiculoServiceMock = new();

    [Fact]
    public async Task MapVeiculoEndpoints_Post_DeveInvocarServico_ComCommandCorreto()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var command = new CriarVeiculoCommand
        {
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var veiculoViewModel = new VeiculoViewModel
        {
            Id = Guid.NewGuid(),
            Placa = "ABC1D23"
        };

        _veiculoServiceMock
            .Setup(s => s.CriarAsync(It.Is<CriarVeiculoCommand>(cmd => cmd.Placa == "ABC1D23"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoViewModel);

        await veiculoController.Post(command, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.CriarAsync(It.Is<CriarVeiculoCommand>(cmd => cmd.Placa == "ABC1D23"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_GetById_DeveInvocarServicoComIdCorreto()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var veiculoId = Guid.NewGuid();
        var veiculoViewModel = new VeiculoViewModel { Id = veiculoId, Placa = "ABC1D23" };

        _veiculoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == veiculoId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoViewModel);

        await veiculoController.GetById(veiculoId, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == veiculoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Get_DeveInvocarServicoComQuery()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        IReadOnlyCollection<VeiculoViewModel> veiculos = new List<VeiculoViewModel>
        {
            new() { Id = Guid.NewGuid(), Placa = "ABC1D23" }
        };

        _veiculoServiceMock
            .Setup(s => s.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculos);

        await veiculoController.Get(null, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Put_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var veiculoId = Guid.NewGuid();
        var command = new AtualizarVeiculoCommand
        {
            Id = veiculoId,
            Placa = "XYZ9W88",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2024,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var veiculoViewModel = new VeiculoViewModel { Id = veiculoId, Placa = "XYZ9W88" };

        _veiculoServiceMock
            .Setup(s => s.AtualizarAsync(It.Is<AtualizarVeiculoCommand>(cmd => cmd.Id == veiculoId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoViewModel);

        await veiculoController.Put(command, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.AtualizarAsync(It.Is<AtualizarVeiculoCommand>(cmd => cmd.Id == veiculoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Delete_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var veiculoId = Guid.NewGuid();

        _veiculoServiceMock
            .Setup(s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(cmd => cmd.Id == veiculoId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await veiculoController.Delete(veiculoId, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(cmd => cmd.Id == veiculoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Post_DevePassarCancellationToken()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var command = new CriarVeiculoCommand
        {
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = Guid.NewGuid()
        };
        var cts = new CancellationTokenSource();

        _veiculoServiceMock
            .Setup(s => s.CriarAsync(It.IsAny<CriarVeiculoCommand>(), cts.Token))
            .ReturnsAsync(new VeiculoViewModel());

        await veiculoController.Post(command, cts.Token);

        _veiculoServiceMock.Verify(
            s => s.CriarAsync(It.IsAny<CriarVeiculoCommand>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_GetById_DeveRepassarIdExatamente()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var veiculoId = Guid.NewGuid();

        _veiculoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.IsAny<ObterVeiculoPorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VeiculoViewModel());

        await veiculoController.GetById(veiculoId, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.ObterPorIdAsync(
                It.Is<ObterVeiculoPorIdQuery>(q => q.Id == veiculoId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Put_DeveRepassarComandoInteiro()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
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

        _veiculoServiceMock
            .Setup(s => s.AtualizarAsync(It.IsAny<AtualizarVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VeiculoViewModel());

        await veiculoController.Put(command, CancellationToken.None);

        _veiculoServiceMock.Verify(
            s => s.AtualizarAsync(
                It.Is<AtualizarVeiculoCommand>(cmd =>
                    cmd.Placa == "ABC1D23" &&
                    cmd.Marca == "Toyota" &&
                    cmd.Modelo == "Corolla" &&
                    cmd.Renavam == "12345678901"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapVeiculoEndpoints_Delete_DeveRepassarIdCorretamente()
    {
        var adapter = new VeiculoAdapter();
        var veiculoController = new VeiculoController(_veiculoServiceMock.Object, adapter);
        var veiculoId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _veiculoServiceMock
            .Setup(s => s.ExcluirAsync(It.IsAny<ExcluirVeiculoCommand>(), cts.Token))
            .Returns(Task.CompletedTask);

        await veiculoController.Delete(veiculoId, cts.Token);

        _veiculoServiceMock.Verify(
            s => s.ExcluirAsync(
                It.Is<ExcluirVeiculoCommand>(cmd => cmd.Id == veiculoId),
                cts.Token),
            Times.Once);
    }
}
