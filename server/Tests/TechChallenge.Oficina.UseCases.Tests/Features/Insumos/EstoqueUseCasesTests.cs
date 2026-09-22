using Moq;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.UseCases.Tests.Features.Insumos;

public sealed class EstoqueUseCasesTests
{
    private readonly Mock<IInsumoGateway> _insumoRepositoryMock = new();

    private EstoqueUseCases CriarService() => new(_insumoRepositoryMock.Object);

    private static Servico CriarServicoComItens(string nome, params (Insumo insumo, int quantidade)[] itens)
    {
        var itensServico = itens
            .Select(item => ItemServico.Criar(item.insumo, item.quantidade))
            .ToArray();

        return Servico.Criar(nome, "Servico completo", itensServico);
    }

    [Fact]
    public async Task VerificarDisponibilidadeParaOrcamentoAsync_DevePassar_QuandoEstoqueSuficiente()
    {
        var service = CriarService();
        var insumoOleo = Insumo.Criar("Oleo", "Bosch", 10, 19.9m);
        var insumoFiltro = Insumo.Criar("Filtro", "Fram", 5, 39.9m);

        var servicos = new[]
        {
            CriarServicoComItens("Revisao", (insumoOleo, 2), (insumoFiltro, 1)),
            CriarServicoComItens("Troca de Oleo", (insumoOleo, 3))
        };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoOleo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumoOleo);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoFiltro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumoFiltro);

        await service.VerificarDisponibilidadeParaOrcamentoAsync(servicos);

        _insumoRepositoryMock.Verify(r => r.ObterPorIdAsync(insumoOleo.Id, It.IsAny<CancellationToken>()), Times.Once);
        _insumoRepositoryMock.Verify(r => r.ObterPorIdAsync(insumoFiltro.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerificarDisponibilidadeParaOrcamentoAsync_DeveLancarDomainException_QuandoEstoqueInsuficiente()
    {
        var service = CriarService();
        var insumoOleo = Insumo.Criar("Oleo", "Bosch", 4, 19.9m);
        var servicos = new[]
        {
            CriarServicoComItens("Revisao", (insumoOleo, 2)),
            CriarServicoComItens("Troca de Oleo", (insumoOleo, 3))
        };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoOleo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumoOleo);

        var action = async () => await service.VerificarDisponibilidadeParaOrcamentoAsync(servicos);

        await Assert.ThrowsAsync<DomainException>(action);
    }

    [Fact]
    public async Task VerificarDisponibilidadeParaOrcamentoAsync_DeveLancarQuandoInsumoNaoEncontrado()
    {
        var service = CriarService();
        var insumo = Insumo.Criar("Pastilha", "Cobreq", 10, 59.9m);
        var servicos = new[] { CriarServicoComItens("Freio", (insumo, 2)) };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Insumo?)null);

        var action = async () => await service.VerificarDisponibilidadeParaOrcamentoAsync(servicos);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal($"Insumo com ID '{insumo.Id}' nao encontrado.", exception.Message);
    }

    [Fact]
    public async Task DebitarEstoqueParaOrdemServicoAsync_DeveDebitarEAualizarInsumos()
    {
        var service = CriarService();
        var insumoOleo = Insumo.Criar("Oleo", "Bosch", 10, 19.9m);
        var insumoFiltro = Insumo.Criar("Filtro", "Fram", 5, 39.9m);
        var servicos = new[]
        {
            CriarServicoComItens("Revisao", (insumoOleo, 2), (insumoFiltro, 1)),
            CriarServicoComItens("Troca de Oleo", (insumoOleo, 3))
        };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoOleo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumoOleo);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoFiltro.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumoFiltro);

        await service.DebitarEstoqueParaOrdemServicoAsync(servicos);

        Assert.Equal(5, insumoOleo.QuantidadeDisponivel);
        Assert.Equal(4, insumoFiltro.QuantidadeDisponivel);
        _insumoRepositoryMock.Verify(r => r.AtualizarAsync(insumoOleo, It.IsAny<CancellationToken>()), Times.Once);
        _insumoRepositoryMock.Verify(r => r.AtualizarAsync(insumoFiltro, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DebitarEstoqueParaOrdemServicoAsync_DeveLancarDomainException_QuandoEstoqueInsuficiente()
    {
        var service = CriarService();
        var insumo = Insumo.Criar("Oleo", "Bosch", 2, 19.9m);
        var servicos = new[] { CriarServicoComItens("Troca de Oleo", (insumo, 3)) };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumo);

        var action = async () => await service.DebitarEstoqueParaOrdemServicoAsync(servicos);

        await Assert.ThrowsAsync<DomainException>(action);
        _insumoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DebitarEstoqueParaOrdemServicoAsync_DeveLancarQuandoInsumoNaoEncontrado()
    {
        var service = CriarService();
        var insumo = Insumo.Criar("Filtro", "Fram", 10, 39.9m);
        var servicos = new[] { CriarServicoComItens("Revisao", (insumo, 1)) };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Insumo?)null);

        var action = async () => await service.DebitarEstoqueParaOrdemServicoAsync(servicos);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal($"Insumo com ID '{insumo.Id}' nao encontrado.", exception.Message);
    }
}
