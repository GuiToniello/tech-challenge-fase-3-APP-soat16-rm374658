using AutoMapper;
using Moq;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;

namespace TechChallenge.Oficina.UseCases.Tests.Features.UseCases;

public sealed class VeiculoUseCasesTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IVeiculoGateway> _veiculoRepositoryMock = new();
    private readonly Mock<IClienteGateway> _clienteRepositoryMock = new();

    private VeiculoUseCases CriarService() =>
        new(_mapperMock.Object, _veiculoRepositoryMock.Object, _clienteRepositoryMock.Object);

    private static Cliente CriarCliente() =>
        Cliente.Criar("Cliente Teste", IdentificacaoCliente.Criar("52998224725"));

    private static Veiculo CriarVeiculo(Guid clienteId) =>
        Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", clienteId);

    private static VeiculoViewModel MapearViewModel(Veiculo veiculo) =>
        new() { Id = veiculo.Id, Placa = veiculo.Placa.Valor, Marca = veiculo.Marca, Modelo = veiculo.Modelo, Ano = veiculo.Ano, Renavam = veiculo.Renavam, ClienteId = veiculo.ClienteId };

    [Fact]
    public async Task CriarAsync_DeveAdicionarVeiculoERetornarViewModel()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = cliente.Id };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ExisteComPlacaAsync("ABC1D23", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<VeiculoViewModel>(It.IsAny<object>())).Returns((object s) => MapearViewModel((Veiculo)s));

        var resultado = await service.CriarAsync(command);

        Assert.Equal("ABC1D23", resultado.Placa);
        _veiculoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarKeyNotFoundException_QuandoClienteNaoExiste()
    {
        var service = CriarService();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(command.ClienteId, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Cliente não encontrado.", exception.Message);
        _veiculoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoPlacaDuplicada()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = cliente.Id };

        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ExisteComPlacaAsync("ABC1D23", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Já existe um veículo cadastrado com a placa informada.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarVeiculoQuandoDadosValidos()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculoExistente = CriarVeiculo(cliente.Id);
        var command = new AtualizarVeiculoCommand { Id = veiculoExistente.Id, Placa = "XYZ9A00", Marca = "Honda", Modelo = "Civic", Ano = 2024, Renavam = "98765432100", ClienteId = cliente.Id };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculoExistente);
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ExisteComPlacaAsync("XYZ9A00", veiculoExistente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<VeiculoViewModel>(It.IsAny<object>())).Returns((object s) => MapearViewModel((Veiculo)s));

        var resultado = await service.AtualizarAsync(command);

        Assert.Equal("XYZ9A00", resultado.Placa);
        _veiculoRepositoryMock.Verify(r => r.AtualizarAsync(veiculoExistente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarKeyNotFoundException_QuandoVeiculoNaoExiste()
    {
        var service = CriarService();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Veículo não encontrado.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarDomainException_QuandoPlacaDuplicada()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculoExistente = CriarVeiculo(cliente.Id);
        var command = new AtualizarVeiculoCommand { Id = veiculoExistente.Id, Placa = "XYZ9A00", Marca = "Honda", Modelo = "Civic", Ano = 2024, Renavam = "98765432100", ClienteId = cliente.Id };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculoExistente);
        _clienteRepositoryMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _veiculoRepositoryMock.Setup(r => r.ExisteComPlacaAsync("XYZ9A00", veiculoExistente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Já existe um veículo cadastrado com a placa informada.", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarViewModel_QuandoExiste()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var query = new ObterVeiculoPorIdQuery { Id = veiculo.Id };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        _mapperMock.Setup(m => m.Map<VeiculoViewModel>(It.IsAny<object>())).Returns((object s) => MapearViewModel((Veiculo)s));

        var resultado = await service.ObterPorIdAsync(query);

        Assert.Equal(veiculo.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarKeyNotFoundException_QuandoNaoExiste()
    {
        var service = CriarService();
        var query = new ObterVeiculoPorIdQuery { Id = Guid.NewGuid() };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var action = async () => await service.ObterPorIdAsync(query);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Veículo não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ListarAsync_SemClienteId_DeveListarTodos()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculos = new[] { CriarVeiculo(cliente.Id) };
        var query = new ListarVeiculosQuery();

        _veiculoRepositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(veiculos);
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<VeiculoViewModel>>(It.IsAny<object>())).Returns(veiculos.Select(MapearViewModel).ToArray());

        var resultado = await service.ListarAsync(query);

        Assert.Single(resultado);
        _veiculoRepositoryMock.Verify(r => r.ListarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarAsync_ComClienteId_DeveListarPorCliente()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculos = new[] { CriarVeiculo(cliente.Id) };
        var query = new ListarVeiculosQuery { ClienteId = cliente.Id };

        _veiculoRepositoryMock.Setup(r => r.ListarPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculos);
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<VeiculoViewModel>>(It.IsAny<object>())).Returns(veiculos.Select(MapearViewModel).ToArray());

        var resultado = await service.ListarAsync(query);

        Assert.Single(resultado);
        _veiculoRepositoryMock.Verify(r => r.ListarPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverVeiculo_QuandoExiste()
    {
        var service = CriarService();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var command = new ExcluirVeiculoCommand { Id = veiculo.Id };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(veiculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        await service.ExcluirAsync(command);

        _veiculoRepositoryMock.Verify(r => r.RemoverAsync(veiculo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_DeveLancarKeyNotFoundException_QuandoNaoExiste()
    {
        var service = CriarService();
        var command = new ExcluirVeiculoCommand { Id = Guid.NewGuid() };

        _veiculoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);

        var action = async () => await service.ExcluirAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Veículo não encontrado.", exception.Message);
    }
}
