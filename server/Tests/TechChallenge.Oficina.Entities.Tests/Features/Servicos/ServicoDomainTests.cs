using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.Entities.Tests.Features.Servicos;

public sealed class ServicoDomainTests
{
    [Fact]
    public void Servico_Criar_DevePreencherDadosEItensServico()
    {
        var insumoA = Insumo.Criar("Filtro de Oleo", "Bosch", 10, 29.90m);
        var insumoB = Insumo.Criar("Vela de Ignicao", "NGK", 15, 19.90m);
        var itemA = ItemServico.Criar(insumoA, 1);
        var itemB = ItemServico.Criar(insumoB, 2);

        var servico = Servico.Criar("Troca de Oleo", "Troca de oleo e filtro", [itemA, itemB]);

        Assert.NotEqual(Guid.Empty, servico.Id);
        Assert.Equal("Troca de Oleo", servico.Nome);
        Assert.Equal("Troca de oleo e filtro", servico.Descricao);
        Assert.Equal(2, servico.ItensServico.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ItemServico_Criar_DeveLancarQuandoQuantidadeInvalida(int quantidade)
    {
        var insumo = Insumo.Criar("Filtro de Oleo", "Bosch", 10, 29.90m);

        var action = () => ItemServico.Criar(insumo, quantidade);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade do item de servico deve ser maior que zero.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Servico_AtualizarNome_DeveLancarQuandoNomeInvalido(string nome)
    {
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        var action = () => servico.AtualizarNome(nome);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome do servico e obrigatorio.", exception.Message);
    }

    [Fact]
    public void Servico_AtualizarNome_DeveLancarQuandoMenorQueTresCaracteres()
    {
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        var action = () => servico.AtualizarNome("ab");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome do servico deve possuir ao menos 3 caracteres.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Servico_AtualizarDescricao_DeveLancarQuandoDescricaoInvalida(string descricao)
    {
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        var action = () => servico.AtualizarDescricao(descricao);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A descricao do servico e obrigatoria.", exception.Message);
    }

    [Fact]
    public void Servico_AtualizarDescricao_DeveLancarQuandoMenorQueTresCaracteres()
    {
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        var action = () => servico.AtualizarDescricao("ab");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A descricao do servico deve possuir ao menos 3 caracteres.", exception.Message);
    }

    [Fact]
    public void Servico_DefinirItensServico_DeveRemoverDuplicadosPorInsumoId()
    {
        var insumo = Insumo.Criar("Filtro de Oleo", "Bosch", 10, 29.90m);
        var itemServicoA = ItemServico.Criar(insumo, 1);
        var itemServicoB = ItemServico.Criar(insumo, 2);
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        servico.DefinirItensServico([itemServicoA, itemServicoB]);

        Assert.Single(servico.ItensServico);
    }

    [Fact]
    public void Servico_DefinirItensServico_DevePermitirColecaoNula()
    {
        var servico = Servico.Criar("Revisao", "Revisao completa", []);

        servico.DefinirItensServico(null);

        Assert.Empty(servico.ItensServico);
    }
}
