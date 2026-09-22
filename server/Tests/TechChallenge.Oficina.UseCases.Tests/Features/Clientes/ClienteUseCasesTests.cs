using AutoMapper;
using Moq;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using Xunit;

namespace TechChallenge.Oficina.UseCases.Tests.Features.Clientes;

public sealed class ClienteUseCasesTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IClienteGateway> _repositoryMock = new();

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoIdentificacaoDuplicada()
    {
        var service = CriarService();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "52998224725" };

        _repositoryMock
            .Setup(repo => repo.ExisteComIdentificacaoAsync("52998224725", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Já existe um cliente cadastrado com a identificação informada.", exception.Message);
        _repositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_DeveAdicionarClienteERetornarViewModel()
    {
        var service = CriarService();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente Teste", Identificacao = "52998224725", Email = "cliente@teste.com" };

        _repositoryMock
            .Setup(repo => repo.ExisteComIdentificacaoAsync("52998224725", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mapperMock
            .Setup(mapper => mapper.Map<ClienteViewModel>(It.IsAny<object>()))
            .Returns((object source) =>
            {
                var cliente = (Cliente)source;
                return new ClienteViewModel
                {
                    Id = cliente.Id,
                    NomeCompleto = cliente.NomeCompleto,
                    Identificacao = cliente.Identificacao.Valor,
                    TipoIdentificacao = cliente.Identificacao.Tipo.ToString(),
                    Email = cliente.Email
                };
            });

        var resultado = await service.CriarAsync(command);

        Assert.Equal("Cliente Teste", resultado.NomeCompleto);
        Assert.Equal("52998224725", resultado.Identificacao);
        Assert.Equal("cliente@teste.com", resultado.Email);
        _repositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarQuandoClienteNaoEncontrado()
    {
        var service = CriarService();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Novo Nome", Identificacao = "52998224725" };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Cliente não encontrado.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarClienteQuandoDadosValidos()
    {
        var service = CriarService();
        var clienteExistente = Cliente.Criar("Nome Antigo", IdentificacaoCliente.Criar("52998224725"));
        var command = new AtualizarClienteCommand
        {
            Id = clienteExistente.Id,
            NomeCompleto = "Nome Atualizado",
            Identificacao = "04252011000110"
        };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);
        _repositoryMock
            .Setup(repo => repo.ExisteComIdentificacaoAsync("04252011000110", clienteExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mapperMock
            .Setup(mapper => mapper.Map<ClienteViewModel>(It.IsAny<object>()))
            .Returns((object source) =>
            {
                var cliente = (Cliente)source;
                return new ClienteViewModel { Id = cliente.Id, NomeCompleto = cliente.NomeCompleto };
            });

        var resultado = await service.AtualizarAsync(command);

        Assert.Equal("Nome Atualizado", resultado.NomeCompleto);
        _repositoryMock.Verify(repo => repo.AtualizarAsync(clienteExistente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarDomainException_QuandoIdentificacaoDuplicada()
    {
        var service = CriarService();
        var clienteExistente = Cliente.Criar("Cliente", IdentificacaoCliente.Criar("52998224725"));
        var command = new AtualizarClienteCommand
        {
            Id = clienteExistente.Id,
            NomeCompleto = "Cliente Atualizado",
            Identificacao = "04252011000110"
        };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);
        _repositoryMock
            .Setup(repo => repo.ExisteComIdentificacaoAsync("04252011000110", clienteExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Já existe um cliente cadastrado com a identificação informada.", exception.Message);
        _repositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarQuandoClienteNaoEncontrado()
    {
        var service = CriarService();
        var query = new ObterClientePorIdQuery { Id = Guid.NewGuid() };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var action = async () => await service.ObterPorIdAsync(query);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Cliente não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarViewModel_QuandoClienteExiste()
    {
        var service = CriarService();
        var clienteExistente = Cliente.Criar("Nome Teste", IdentificacaoCliente.Criar("52998224725"));
        var query = new ObterClientePorIdQuery { Id = clienteExistente.Id };
        var viewModel = new ClienteViewModel { Id = clienteExistente.Id, NomeCompleto = "Nome Teste" };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);
        _mapperMock
            .Setup(mapper => mapper.Map<ClienteViewModel>(clienteExistente))
            .Returns(viewModel);

        var resultado = await service.ObterPorIdAsync(query);

        Assert.Equal(viewModel.Id, resultado.Id);
        Assert.Equal("Nome Teste", resultado.NomeCompleto);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverQuandoClienteExiste()
    {
        var service = CriarService();
        var clienteExistente = Cliente.Criar("Nome", IdentificacaoCliente.Criar("52998224725"));
        var command = new ExcluirClienteCommand { Id = clienteExistente.Id };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(clienteExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        await service.ExcluirAsync(command);

        _repositoryMock.Verify(repo => repo.RemoverAsync(clienteExistente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_DeveLancarKeyNotFoundException_QuandoClienteNaoEncontrado()
    {
        var service = CriarService();
        var command = new ExcluirClienteCommand { Id = Guid.NewGuid() };

        _repositoryMock
            .Setup(repo => repo.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var action = async () => await service.ExcluirAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Cliente não encontrado.", exception.Message);
        _repositoryMock.Verify(repo => repo.RemoverAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarColecaoVazia_QuandoNaoHaClientes()
    {
        var service = CriarService();
        IReadOnlyCollection<ClienteViewModel> viewModels = [];

        _repositoryMock
            .Setup(repo => repo.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cliente>());
        _mapperMock
            .Setup(mapper => mapper.Map<IReadOnlyCollection<ClienteViewModel>>(It.IsAny<object>()))
            .Returns(viewModels);

        var resultado = await service.ListarAsync(new ListarClientesQuery());

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoIdentificacaoInvalida()
    {
        var service = CriarService();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "123" };

        var action = async () => await service.CriarAsync(command);

        await Assert.ThrowsAsync<DomainException>(action);
        _repositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ClienteUseCases CriarService()
    {
        return new ClienteUseCases(_mapperMock.Object, _repositoryMock.Object);
    }
}
