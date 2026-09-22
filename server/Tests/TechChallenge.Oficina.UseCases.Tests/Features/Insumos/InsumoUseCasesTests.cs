using AutoMapper;
using Moq;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using Xunit;

namespace TechChallenge.Oficina.UseCases.Tests.Features.Insumos;

public sealed class InsumoUseCasesTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IInsumoGateway> _repositoryMock = new();

    private InsumoUseCases CriarService() => new(_mapperMock.Object, _repositoryMock.Object);

    private static Insumo CriarInsumo() => Insumo.Criar("Óleo 5W30", "Bosch", 10, 29.9m);

    private static InsumoViewModel MapearViewModel(Insumo insumo) =>
        new() { Id = insumo.Id, Nome = insumo.Nome, Fabricante = insumo.Fabricante, QuantidadeDisponivel = insumo.QuantidadeDisponivel, ValorUnitario = insumo.ValorUnitario };

    [Fact]
    public async Task CriarAsync_DeveAdicionarERetornarViewModel()
    {
        var service = CriarService();
        var command = new CriarInsumoCommand { Nome = "Óleo 5W30", Fabricante = "Bosch", QuantidadeDisponivel = 8, ValorUnitario = 19.9m };

        _mapperMock.Setup(m => m.Map<InsumoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((Insumo)source));

        var resultado = await service.CriarAsync(command);

        Assert.Equal("Óleo 5W30", resultado.Nome);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoDadosInvalidos()
    {
        var service = CriarService();
        var command = new CriarInsumoCommand { Nome = "", Fabricante = "Bosch", QuantidadeDisponivel = 8, ValorUnitario = 19.9m };

        var action = async () => await service.CriarAsync(command);

        await Assert.ThrowsAsync<DomainException>(action);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarQuandoInsumoExiste()
    {
        var service = CriarService();
        var insumo = CriarInsumo();
        var command = new AtualizarInsumoCommand { Id = insumo.Id, Nome = "Filtro de Ar", Fabricante = "Fram", QuantidadeDisponivel = 20, ValorUnitario = 45.5m };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumo);
        _mapperMock.Setup(m => m.Map<InsumoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((Insumo)source));

        var resultado = await service.AtualizarAsync(command);

        Assert.Equal("Filtro de Ar", resultado.Nome);
        _repositoryMock.Verify(r => r.AtualizarAsync(insumo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarQuandoInsumoNaoEncontrado()
    {
        var service = CriarService();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "Filtro", Fabricante = "Fram", QuantidadeDisponivel = 20, ValorUnitario = 45.5m };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Insumo?)null);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Insumo não encontrado.", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarQuandoNaoEncontrado()
    {
        var service = CriarService();
        var query = new ObterInsumoPorIdQuery { Id = Guid.NewGuid() };

        _repositoryMock.Setup(r => r.ObterPorIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Insumo?)null);

        var action = async () => await service.ObterPorIdAsync(query);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarColecaoMapeada()
    {
        var service = CriarService();
        var insumos = new[] { CriarInsumo() };

        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(insumos);
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<InsumoViewModel>>(It.IsAny<object>())).Returns(insumos.Select(MapearViewModel).ToArray());

        var resultado = await service.ListarAsync(new ListarInsumosQuery());

        Assert.Single(resultado);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverQuandoEncontrado()
    {
        var service = CriarService();
        var insumo = CriarInsumo();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumo);

        await service.ExcluirAsync(new ExcluirInsumoCommand { Id = insumo.Id });

        _repositoryMock.Verify(r => r.RemoverAsync(insumo, It.IsAny<CancellationToken>()), Times.Once);
    }
}
