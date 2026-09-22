using Moq;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Adapters.Features.Clientes;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Monolith.API.Tests.Features.Clientes;

public sealed class ClienteEndpointsTests
{
    private readonly Mock<IClienteUseCases> _clienteServiceMock = new();

    [Fact]
    public async Task MapClienteEndpoints_Post_DeveInvocarServico_ComCommandCorreto()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var command = new CriarClienteCommand 
        { 
            NomeCompleto = "João Silva", 
            Identificacao = "12345678900",
            Email = "joao@example.com"
        };
        var clienteViewModel = new ClienteViewModel 
        { 
            Id = Guid.NewGuid(), 
            NomeCompleto = "João Silva"
        };

        _clienteServiceMock
            .Setup(s => s.CriarAsync(It.Is<CriarClienteCommand>(cmd => cmd.NomeCompleto == "João Silva"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteViewModel);

        await clienteController.Post(command, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.CriarAsync(It.Is<CriarClienteCommand>(cmd => cmd.NomeCompleto == "João Silva"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_GetById_DeveInvocarServicoComIdCorreto()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clienteId = Guid.NewGuid();
        var clienteViewModel = new ClienteViewModel { Id = clienteId, NomeCompleto = "Maria" };

        _clienteServiceMock
            .Setup(s => s.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == clienteId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteViewModel);

        await clienteController.GetById(clienteId, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == clienteId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Get_DeveInvocarServicoComQuery()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clientes = new List<ClienteViewModel>
        {
            new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "João" }
        }.AsReadOnly();

        _clienteServiceMock
            .Setup(s => s.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        await clienteController.Get(CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Put_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clienteId = Guid.NewGuid();
        var command = new AtualizarClienteCommand 
        { 
            Id = clienteId,
            NomeCompleto = "João Atualizado", 
            Identificacao = "12345678900"
        };
        var clienteViewModel = new ClienteViewModel { Id = clienteId, NomeCompleto = "João Atualizado" };

        _clienteServiceMock
            .Setup(s => s.AtualizarAsync(It.Is<AtualizarClienteCommand>(cmd => cmd.Id == clienteId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteViewModel);

        await clienteController.Put(command, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.AtualizarAsync(It.Is<AtualizarClienteCommand>(cmd => cmd.Id == clienteId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Delete_DeveInvocarServicoComCommandCorreto()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clienteId = Guid.NewGuid();

        _clienteServiceMock
            .Setup(s => s.ExcluirAsync(It.Is<ExcluirClienteCommand>(cmd => cmd.Id == clienteId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await clienteController.Delete(clienteId, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.ExcluirAsync(It.Is<ExcluirClienteCommand>(cmd => cmd.Id == clienteId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Post_DevePassarCancellationToken()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var command = new CriarClienteCommand { NomeCompleto = "João", Identificacao = "123" };
        var cts = new CancellationTokenSource();

        _clienteServiceMock
            .Setup(s => s.CriarAsync(It.IsAny<CriarClienteCommand>(), cts.Token))
            .ReturnsAsync(new ClienteViewModel());

        await clienteController.Post(command, cts.Token);

        _clienteServiceMock.Verify(
            s => s.CriarAsync(It.IsAny<CriarClienteCommand>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_GetById_DeveRepassarIdExatamente()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clienteId = Guid.NewGuid();

        _clienteServiceMock
            .Setup(s => s.ObterPorIdAsync(It.IsAny<ObterClientePorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClienteViewModel());

        await clienteController.GetById(clienteId, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.ObterPorIdAsync(
                It.Is<ObterClientePorIdQuery>(q => q.Id == clienteId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Put_DeveRepassarComandoInteiro()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var command = new AtualizarClienteCommand 
        { 
            Id = Guid.NewGuid(),
            NomeCompleto = "João", 
            Identificacao = "123",
            Email = "joao@test.com"
        };

        _clienteServiceMock
            .Setup(s => s.AtualizarAsync(It.IsAny<AtualizarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClienteViewModel());

        await clienteController.Put(command, CancellationToken.None);

        _clienteServiceMock.Verify(
            s => s.AtualizarAsync(
                It.Is<AtualizarClienteCommand>(cmd => 
                    cmd.NomeCompleto == "João" &&
                    cmd.Identificacao == "123" &&
                    cmd.Email == "joao@test.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MapClienteEndpoints_Delete_DeveRepassarIdCorretamente()
    {
        var adapter = new ClienteAdapter();
        var clienteController = new Controllers.Features.Clientes.ClienteController(_clienteServiceMock.Object, adapter);
        var clienteId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _clienteServiceMock
            .Setup(s => s.ExcluirAsync(It.IsAny<ExcluirClienteCommand>(), cts.Token))
            .Returns(Task.CompletedTask);

        await clienteController.Delete(clienteId, cts.Token);

        _clienteServiceMock.Verify(
            s => s.ExcluirAsync(
                It.Is<ExcluirClienteCommand>(cmd => cmd.Id == clienteId),
                cts.Token),
            Times.Once);
    }
}
