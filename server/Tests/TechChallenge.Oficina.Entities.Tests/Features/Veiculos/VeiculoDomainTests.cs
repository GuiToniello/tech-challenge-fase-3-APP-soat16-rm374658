using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.Veiculos.VOs;
using Xunit;

namespace TechChallenge.Oficina.Entities.Tests.Features.Veiculos;

public sealed class VeiculoDomainTests
{
    private static readonly Guid ClienteIdValido = Guid.NewGuid();

    [Fact]
    public void Veiculo_Criar_DevePreencherDadosCorretamente()
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        Assert.NotEqual(Guid.Empty, veiculo.Id);
        Assert.Equal("ABC1D23", veiculo.Placa.Valor);
        Assert.Equal("Toyota", veiculo.Marca);
        Assert.Equal("Corolla", veiculo.Modelo);
        Assert.Equal(2023, veiculo.Ano);
        Assert.Equal("12345678901", veiculo.Renavam);
        Assert.Equal(ClienteIdValido, veiculo.ClienteId);
    }

    // --- PlacaMercosul ---

    [Theory]
    [InlineData("ABC1D23")]
    [InlineData("XYZ9A00")]
    [InlineData("ABC-1D23")]
    [InlineData("abc1d23")]
    public void PlacaMercosul_Criar_DeveAceitarPlacaValida(string placa)
    {
        var placaVO = PlacaMercosul.Criar(placa);

        Assert.True(PlacaMercosul.EhValido(placa));
        Assert.Equal(7, placaVO.Valor.Length);
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("AB1D23")]
    [InlineData("ABCD123")]
    [InlineData("ABC1D2")]
    public void PlacaMercosul_Criar_DeveLancarQuandoFormatoInvalido(string placa)
    {
        void Action() => PlacaMercosul.Criar(placa);

        var exception = Assert.Throws<DomainException>(Action);
        Assert.Equal("A placa informada deve estar no padrão Mercosul (ex: ABC1D23).", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void PlacaMercosul_Criar_DeveLancarQuandoVazia(string placa)
    {
        void Action() => PlacaMercosul.Criar(placa);

        var exception = Assert.Throws<DomainException>(Action);
        Assert.Equal("A placa do veículo é obrigatória.", exception.Message);
    }

    [Fact]
    public void PlacaMercosul_EhValido_DeveRetornarFalseQuandoVazia()
    {
        Assert.False(PlacaMercosul.EhValido(""));
        Assert.False(PlacaMercosul.EhValido("  "));
    }

    [Fact]
    public void PlacaMercosul_EhValido_DeveRetornarFalseQuandoInvalida()
    {
        Assert.False(PlacaMercosul.EhValido("ABC1234"));
    }

    [Fact]
    public void PlacaMercosul_Equals_DeveSerIgual_QuandoMesmoValor()
    {
        var p1 = PlacaMercosul.Criar("ABC1D23");
        var p2 = PlacaMercosul.Criar("ABC1D23");

        Assert.Equal(p1, p2);
        Assert.True(p1.Equals((object)p2));
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void PlacaMercosul_ToString_DeveRetornarValor()
    {
        var placa = PlacaMercosul.Criar("ABC1D23");
        Assert.Equal("ABC1D23", placa.ToString());
    }

    // --- Veiculo validações ---

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Veiculo_AtualizarMarca_DeveLancarQuandoVazia(string marca)
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarMarca(marca);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A marca do veículo é obrigatória.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Veiculo_AtualizarModelo_DeveLancarQuandoVazio(string modelo)
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarModelo(modelo);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O modelo do veículo é obrigatório.", exception.Message);
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(3000)]
    public void Veiculo_AtualizarAno_DeveLancarQuandoAnoInvalido(int ano)
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarAno(ano);

        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Veiculo_AtualizarRenavam_DeveLancarQuandoVazio(string renavam)
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarRenavam(renavam);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O RENAVAM do veículo é obrigatório.", exception.Message);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789012")]
    [InlineData("ABCDEFGHIJ")]
    public void Veiculo_AtualizarRenavam_DeveLancarQuandoInvalido(string renavam)
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarRenavam(renavam);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O RENAVAM informado é inválido.", exception.Message);
    }

    [Fact]
    public void Veiculo_AtualizarClienteId_DeveLancarQuandoGuidVazio()
    {
        var veiculo = Veiculo.Criar("ABC1D23", "Toyota", "Corolla", 2023, "12345678901", ClienteIdValido);

        var action = () => veiculo.AtualizarClienteId(Guid.Empty);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O cliente do veículo é obrigatório.", exception.Message);
    }
}
