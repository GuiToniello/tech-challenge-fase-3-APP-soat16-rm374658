using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;
using Xunit;

namespace TechChallenge.Oficina.Entities.Tests.Features.Insumos;

public sealed class InsumoDomainTests
{
    [Fact]
    public void Insumo_Criar_DeveNormalizarValoresEPreencherDados()
    {
        var insumo = Insumo.Criar("  Filtro de Óleo  ", "  Bosch  ", 10, 12.345m);

        Assert.NotEqual(Guid.Empty, insumo.Id);
        Assert.Equal("Filtro de Óleo", insumo.Nome);
        Assert.Equal("Bosch", insumo.Fabricante);
        Assert.Equal(10, insumo.QuantidadeDisponivel);
        Assert.Equal(12.34m, insumo.ValorUnitario);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Insumo_AtualizarNome_DeveLancarQuandoNomeInvalido(string nome)
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.AtualizarNome(nome);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome do insumo é obrigatório.", exception.Message);
    }

    [Fact]
    public void Insumo_AtualizarNome_DeveLancarQuandoMenorQueTresCaracteres()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.AtualizarNome("ab");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome do insumo deve possuir ao menos 3 caracteres.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Insumo_AtualizarFabricante_DeveLancarQuandoInvalido(string fabricante)
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.AtualizarFabricante(fabricante);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O fabricante do insumo é obrigatório.", exception.Message);
    }

    [Fact]
    public void Insumo_AtualizarNome_DeveLancarQuandoMaiorQueCento50Caracteres()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);
        var nomeInvalido = new string('A', 151);

        var action = () => insumo.AtualizarNome(nomeInvalido);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome do insumo deve ter no máximo 150 caracteres.", exception.Message);
    }

    [Fact]
    public void Insumo_AtualizarNome_DeveAceitarNomeComExatamente150Caracteres()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);
        var nomeValido = new string('A', 150);

        insumo.AtualizarNome(nomeValido);

        Assert.Equal(nomeValido, insumo.Nome);
    }

    [Fact]
    public void Insumo_AtualizarFabricante_DeveLancarQuandoMaiorQueCento50Caracteres()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);
        var fabricanteInvalido = new string('B', 151);

        var action = () => insumo.AtualizarFabricante(fabricanteInvalido);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O fabricante do insumo deve ter no máximo 150 caracteres.", exception.Message);
    }

    [Fact]
    public void Insumo_AtualizarFabricante_DeveAceitarFabricanteComExatamente150Caracteres()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);
        var fabricanteValido = new string('B', 150);

        insumo.AtualizarFabricante(fabricanteValido);

        Assert.Equal(fabricanteValido, insumo.Fabricante);
    }

    [Fact]
    public void Insumo_AtualizarQuantidadeDisponivel_DeveLancarQuandoNegativa()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.AtualizarQuantidadeDisponivel(-1);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade disponível do insumo não pode ser negativa.", exception.Message);
    }

    [Fact]
    public void Insumo_AtualizarValorUnitario_DeveLancarQuandoNegativo()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.AtualizarValorUnitario(-0.01m);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O valor unitário do insumo não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void Insumo_VerificarDisponibilidade_DevePassarQuandoEstoqueSuficiente()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        insumo.VerificarDisponibilidade(5);
        insumo.VerificarDisponibilidade(10);

        Assert.Equal(10, insumo.QuantidadeDisponivel);
    }

    [Fact]
    public void Insumo_VerificarDisponibilidade_DeveLancarQuandoEstoqueInsuficiente()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 5, 19.9m);

        var action = () => insumo.VerificarDisponibilidade(10);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Estoque insuficiente do insumo 'Óleo'. Disponivel: 5, Necessario: 10.", exception.Message);
    }

    [Fact]
    public void Insumo_VerificarDisponibilidade_DeveLancarQuandoQuantidadeZero()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.VerificarDisponibilidade(0);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade necessaria deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Insumo_VerificarDisponibilidade_DeveLancarQuandoQuantidadeNegativa()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.VerificarDisponibilidade(-5);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade necessaria deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Insumo_VerificarDisponibilidade_DeveLancarQuandoEstoqueZero()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 0, 19.9m);

        var action = () => insumo.VerificarDisponibilidade(1);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Estoque insuficiente do insumo 'Óleo'. Disponivel: 0, Necessario: 1.", exception.Message);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DeveDecrementarQuantidadeDisponivel()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        insumo.DebitarEstoque(3);

        Assert.Equal(7, insumo.QuantidadeDisponivel);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DevePermitirDebitarTodoEstoque()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        insumo.DebitarEstoque(10);

        Assert.Equal(0, insumo.QuantidadeDisponivel);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DeveLancarQuandoEstoqueInsuficiente()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 5, 19.9m);

        var action = () => insumo.DebitarEstoque(10);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Estoque insuficiente para debito do insumo 'Óleo'. Disponivel: 5, Solicitado: 10.", exception.Message);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DeveLancarQuandoQuantidadeZero()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.DebitarEstoque(0);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade a debitar deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DeveLancarQuandoQuantidadeNegativa()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 10, 19.9m);

        var action = () => insumo.DebitarEstoque(-5);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A quantidade a debitar deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DeveLancarQuandoEstoqueZero()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 0, 19.9m);

        var action = () => insumo.DebitarEstoque(1);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Estoque insuficiente para debito do insumo 'Óleo'. Disponivel: 0, Solicitado: 1.", exception.Message);
    }

    [Fact]
    public void Insumo_DebitarEstoque_DevePermitirMultiplosDebitos()
    {
        var insumo = Insumo.Criar("Óleo", "Bosch", 20, 19.9m);

        insumo.DebitarEstoque(5);
        insumo.DebitarEstoque(3);
        insumo.DebitarEstoque(2);

        Assert.Equal(10, insumo.QuantidadeDisponivel);
    }
}
