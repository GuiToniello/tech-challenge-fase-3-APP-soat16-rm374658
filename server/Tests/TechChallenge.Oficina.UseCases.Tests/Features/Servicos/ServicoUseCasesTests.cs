using AutoMapper;
using Moq;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.UseCases.Tests.Features.UseCases;

public sealed class ServicoUseCasesTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IServicoGateway> _servicoRepositoryMock = new();
    private readonly Mock<IInsumoGateway> _insumoRepositoryMock = new();

    private ServicoUseCases CriarService() => new(_mapperMock.Object, _servicoRepositoryMock.Object, _insumoRepositoryMock.Object);

    private static Insumo CriarInsumo(string nome = "Filtro") => Insumo.Criar(nome, "Bosch", 10, 10m);

    private static Servico CriarServico(IReadOnlyCollection<ItemServico>? itensServico = null) =>
        Servico.Criar("Revisao", "Revisao geral", itensServico ?? []);

    private static ServicoViewModel MapearViewModel(Servico servico) =>
        new()
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            ItensServico = servico.ItensServico
                .Select(i => new ItemServicoViewModel { InsumoId = i.InsumoId, InsumoNome = i.Insumo.Nome, Quantidade = i.Quantidade })
                .ToArray()
        };

    [Fact]
    public async Task CriarAsync_DeveAdicionarServicoERetornarViewModel()
    {
        var service = CriarService();
        var insumo = CriarInsumo();
        var command = new CriarServicoCommand
        {
            Nome = "Troca de Oleo",
            Descricao = "Troca completa",
            ItensServico = [new ItemServicoCommand { InsumoId = insumo.Id, Quantidade = 2 }]
        };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumo);
        _mapperMock.Setup(m => m.Map<ServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((Servico)source));

        var resultado = await service.CriarAsync(command);

        Assert.Equal("Troca de Oleo", resultado.Nome);
        Assert.Single(resultado.ItensServico);
        Assert.Equal(2, resultado.ItensServico.Single().Quantidade);
        _servicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Servico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoInsumoIdForInvalido()
    {
        var service = CriarService();
        var command = new CriarServicoCommand
        {
            Nome = "Troca",
            Descricao = "Descricao",
            ItensServico = [new ItemServicoCommand { InsumoId = Guid.Empty, Quantidade = 1 }]
        };

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Todos os insumos informados devem possuir identificador valido.", exception.Message);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarDomainException_QuandoQuantidadeForInvalida()
    {
        var service = CriarService();
        var command = new CriarServicoCommand
        {
            Nome = "Troca",
            Descricao = "Descricao",
            ItensServico = [new ItemServicoCommand { InsumoId = Guid.NewGuid(), Quantidade = 0 }]
        };

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<DomainException>(action);
        Assert.Equal("Todos os itens de servico devem possuir quantidade maior que zero.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarServico_QuandoEncontrado()
    {
        var service = CriarService();
        var servico = CriarServico();
        var insumo = CriarInsumo();
        var command = new AtualizarServicoCommand
        {
            Id = servico.Id,
            Nome = "Alinhamento",
            Descricao = "Alinhamento completo",
            ItensServico = [new ItemServicoCommand { InsumoId = insumo.Id, Quantidade = 3 }]
        };

        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);
        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(insumo);
        _mapperMock.Setup(m => m.Map<ServicoViewModel>(It.IsAny<object>())).Returns((object source) => MapearViewModel((Servico)source));

        var resultado = await service.AtualizarAsync(command);

        Assert.Equal("Alinhamento", resultado.Nome);
        Assert.Single(resultado.ItensServico);
        Assert.Equal(3, resultado.ItensServico.Single().Quantidade);
        _servicoRepositoryMock.Verify(r => r.AtualizarAsync(servico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarKeyNotFoundException_QuandoServicoNaoExiste()
    {
        var service = CriarService();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Descricao", ItensServico = [] };

        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var action = async () => await service.AtualizarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Servico nao encontrado.", exception.Message);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarQuandoNaoEncontrado()
    {
        var service = CriarService();
        var query = new ObterServicoPorIdQuery { Id = Guid.NewGuid() };

        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Servico?)null);

        var action = async () => await service.ObterPorIdAsync(query);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarColecaoMapeada()
    {
        var service = CriarService();
        var servicos = new[] { CriarServico() };

        _servicoRepositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(servicos);
        _mapperMock.Setup(m => m.Map<IReadOnlyCollection<ServicoViewModel>>(It.IsAny<object>())).Returns(servicos.Select(MapearViewModel).ToArray());

        var resultado = await service.ListarAsync(new ListarServicosQuery());

        Assert.Single(resultado);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverQuandoEncontrado()
    {
        var service = CriarService();
        var servico = CriarServico();

        _servicoRepositoryMock.Setup(r => r.ObterPorIdAsync(servico.Id, It.IsAny<CancellationToken>())).ReturnsAsync(servico);

        await service.ExcluirAsync(new ExcluirServicoCommand { Id = servico.Id });

        _servicoRepositoryMock.Verify(r => r.RemoverAsync(servico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarKeyNotFoundException_QuandoInsumoNaoExiste()
    {
        var service = CriarService();
        var insumoId = Guid.NewGuid();
        var command = new CriarServicoCommand
        {
            Nome = "Troca",
            Descricao = "Descricao",
            ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
        };

        _insumoRepositoryMock.Setup(r => r.ObterPorIdAsync(insumoId, It.IsAny<CancellationToken>())).ReturnsAsync((Insumo?)null);

        var action = async () => await service.CriarAsync(command);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Equal("Insumo nao encontrado.", exception.Message);
    }
}
