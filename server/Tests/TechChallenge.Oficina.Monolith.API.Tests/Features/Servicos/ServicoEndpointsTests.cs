using Moq;
using TechChallenge.Oficina.Adapters.Features.Servicos;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.Monolith.API.Tests.Features.Servicos;

public sealed class ServicoEndpointsTests
{
    private readonly Mock<IServicoUseCases> _servicoServiceMock = new();

    [Fact]
    public async Task MapServicoEndpoints_Post_DeveInvocarServico_ComCommandCorreto()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var servicoViewModel = new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" };

        _servicoServiceMock
            .Setup(s => s.CriarAsync(It.Is<CriarServicoCommand>(cmd => cmd.Nome == "Troca"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicoViewModel);

        await servicoController.Post(command, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.CriarAsync(It.Is<CriarServicoCommand>(cmd => cmd.Nome == "Troca"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_GetById_DeveInvocarServicoComIdCorreto()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var servicoId = Guid.NewGuid();
        var servicoViewModel = new ServicoViewModel { Id = servicoId, Nome = "Troca" };

        _servicoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == servicoId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicoViewModel);

        await servicoController.GetById(servicoId, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == servicoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Get_DeveInvocarServicoComQuery()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        IReadOnlyCollection<ServicoViewModel> servicos = [new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" }];

        _servicoServiceMock
            .Setup(s => s.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicos);

        await servicoController.Get(CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Put_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var servicoId = Guid.NewGuid();
        var command = new AtualizarServicoCommand { Id = servicoId, Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var servicoViewModel = new ServicoViewModel { Id = servicoId, Nome = "Troca" };

        _servicoServiceMock
            .Setup(s => s.AtualizarAsync(It.Is<AtualizarServicoCommand>(cmd => cmd.Id == servicoId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(servicoViewModel);

        await servicoController.Put(command, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.AtualizarAsync(It.Is<AtualizarServicoCommand>(cmd => cmd.Id == servicoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Delete_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var servicoId = Guid.NewGuid();

        _servicoServiceMock
            .Setup(s => s.ExcluirAsync(It.Is<ExcluirServicoCommand>(cmd => cmd.Id == servicoId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await servicoController.Delete(servicoId, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.ExcluirAsync(It.Is<ExcluirServicoCommand>(cmd => cmd.Id == servicoId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Post_DevePassarCancellationToken()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var cts = new CancellationTokenSource();

        _servicoServiceMock
            .Setup(s => s.CriarAsync(It.IsAny<CriarServicoCommand>(), cts.Token))
            .ReturnsAsync(new ServicoViewModel());

        await servicoController.Post(command, cts.Token);

        _servicoServiceMock.Verify(
            s => s.CriarAsync(It.IsAny<CriarServicoCommand>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_GetById_DeveRepassarIdExatamente()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var servicoId = Guid.NewGuid();

        _servicoServiceMock
            .Setup(s => s.ObterPorIdAsync(It.IsAny<ObterServicoPorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServicoViewModel());

        await servicoController.GetById(servicoId, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.ObterPorIdAsync(
                It.Is<ObterServicoPorIdQuery>(q => q.Id == servicoId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Put_DeveRepassarComandoInteiro()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var command = new AtualizarServicoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Troca",
            Descricao = "Descricao",
            ItensServico = []
        };

        _servicoServiceMock
            .Setup(s => s.AtualizarAsync(It.IsAny<AtualizarServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServicoViewModel());

        await servicoController.Put(command, CancellationToken.None);

        _servicoServiceMock.Verify(
            s => s.AtualizarAsync(
                It.Is<AtualizarServicoCommand>(cmd =>
                    cmd.Nome == "Troca" &&
                    cmd.Descricao == "Descricao"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapServicoEndpoints_Delete_DeveRepassarIdCorretamente()
    {
        var adapter = new ServicoAdapter();
        var servicoController = new ServicoController(_servicoServiceMock.Object, adapter);
        var servicoId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _servicoServiceMock
            .Setup(s => s.ExcluirAsync(It.IsAny<ExcluirServicoCommand>(), cts.Token))
            .Returns(Task.CompletedTask);

        await servicoController.Delete(servicoId, cts.Token);

        _servicoServiceMock.Verify(
            s => s.ExcluirAsync(
                It.Is<ExcluirServicoCommand>(cmd => cmd.Id == servicoId),
                cts.Token),
            Times.Once);
    }
}
