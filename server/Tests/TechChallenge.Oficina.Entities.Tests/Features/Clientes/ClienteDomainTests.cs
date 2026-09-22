using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.Enums;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using Xunit;

namespace TechChallenge.Oficina.Entities.Tests.Features.Clientes;

public sealed class ClienteDomainTests
{
    [Fact]
    public void Cliente_Criar_DeveNormalizarNomeEPreencherDados()
    {
        var identificacao = IdentificacaoCliente.Criar("529.982.247-25");

        var cliente = Cliente.Criar("  Maria Silva  ", identificacao);

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("Maria Silva", cliente.NomeCompleto);
        Assert.Equal(identificacao, cliente.Identificacao);
    }

    [Fact]
    public void Cliente_Criar_DeveNormalizarEmail_QuandoInformado()
    {
        var cliente = Cliente.Criar("Nome Válido", IdentificacaoCliente.Criar("52998224725"), "  cliente@teste.com ");

        Assert.Equal("cliente@teste.com", cliente.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Cliente_AtualizarNomeCompleto_DeveLancarQuandoNomeInvalido(string nome)
    {
        var cliente = Cliente.Criar("Nome Válido", IdentificacaoCliente.Criar("52998224725"));

        var action = () => cliente.AtualizarNomeCompleto(nome);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome completo do cliente é obrigatório.", exception.Message);
    }

    [Fact]
    public void Cliente_AtualizarNomeCompleto_DeveLancarQuandoNomeMenorQueTresCaracteres()
    {
        var cliente = Cliente.Criar("Nome Válido", IdentificacaoCliente.Criar("52998224725"));

        var action = () => cliente.AtualizarNomeCompleto("ab");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("O nome completo do cliente deve possuir ao menos 3 caracteres.", exception.Message);
    }

    [Fact]
    public void Cliente_AtualizarIdentificacao_DeveLancarQuandoNula()
    {
        var cliente = Cliente.Criar("Nome Válido", IdentificacaoCliente.Criar("52998224725"));

        var action = () => cliente.AtualizarIdentificacao(null!);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A identificação do cliente é obrigatória.", exception.Message);
    }

    [Fact]
    public void Cpf_Criar_DeveNormalizarValor()
    {
        var cpf = Cpf.Criar("529.982.247-25");

        Assert.Equal("52998224725", cpf.Valor);
        Assert.True(Cpf.EhValido(cpf.Valor));
    }

    [Fact]
    public void Cpf_Criar_DeveLancarQuandoInvalido()
    {
        void Action() => Cpf.Criar("111.111.111-11");

        var exception = Assert.Throws<DomainException>(Action);
        Assert.Equal("A identificação informada deve ser um CPF válido.", exception.Message);
    }

    [Fact]
    public void Cnpj_Criar_DeveNormalizarValor()
    {
        var cnpj = Cnpj.Criar("04.252.011/0001-10");

        Assert.Equal("04252011000110", cnpj.Valor);
        Assert.True(Cnpj.EhValido(cnpj.Valor));
    }

    [Fact]
    public void Cnpj_Criar_DeveLancarQuandoInvalido()
    {
        void Action() => Cnpj.Criar("11.111.111/1111-11");

        var exception = Assert.Throws<DomainException>(Action);
        Assert.Equal("A identificação informada deve ser um CNPJ válido.", exception.Message);
    }

    [Theory]
    [InlineData("529.982.247-25", TipoIdentificacaoCliente.Cpf, "52998224725")]
    [InlineData("04.252.011/0001-10", TipoIdentificacaoCliente.Cnpj, "04252011000110")]
    public void IdentificacaoCliente_Criar_DeveDefinirTipoCorreto(string valor, TipoIdentificacaoCliente tipo, string normalizado)
    {
        var identificacao = IdentificacaoCliente.Criar(valor);

        Assert.Equal(tipo, identificacao.Tipo);
        Assert.Equal(normalizado, identificacao.Valor);
    }

    [Fact]
    public void IdentificacaoCliente_Criar_DeveLancarQuandoValorInvalido()
    {
        var action = () => IdentificacaoCliente.Criar("123");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A identificação informada deve ser um CPF ou CNPJ válido.", exception.Message);
    }

    [Fact]
    public void Cliente_Criar_DeveAtualizarNomeComSucesso()
    {
        var cliente = Cliente.Criar("Nome Inicial", IdentificacaoCliente.Criar("52998224725"));

        cliente.AtualizarNomeCompleto("  Nome Atualizado  ");

        Assert.Equal("Nome Atualizado", cliente.NomeCompleto);
    }

    [Fact]
    public void Cliente_AtualizarIdentificacao_DeveAtualizarComSucesso()
    {
        var cliente = Cliente.Criar("Nome Válido", IdentificacaoCliente.Criar("52998224725"));
        var novaIdentificacao = IdentificacaoCliente.Criar("04.252.011/0001-10");

        cliente.AtualizarIdentificacao(novaIdentificacao);

        Assert.Equal("04252011000110", cliente.Identificacao.Valor);
        Assert.Equal(TipoIdentificacaoCliente.Cnpj, cliente.Identificacao.Tipo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cpf_Criar_DeveLancarQuandoVazio(string valor)
    {
        var exception = Assert.Throws<DomainException>(() => { Cpf.Criar(valor); });
        Assert.Equal("A identificação do cliente é obrigatória.", exception.Message);
    }

    [Fact]
    public void Cpf_EhValido_DeveRetornarFalse_QuandoVazio()
    {
        Assert.False(Cpf.EhValido(""));
        Assert.False(Cpf.EhValido("   "));
    }

    [Fact]
    public void Cpf_EhValido_DeveRetornarFalse_QuandoInvalido()
    {
        Assert.False(Cpf.EhValido("111.111.111-11"));
        Assert.False(Cpf.EhValido("123"));
    }

    [Fact]
    public void Cpf_Equals_DeveRetornarTrue_QuandoMesmoValor()
    {
        var cpf1 = Cpf.Criar("529.982.247-25");
        var cpf2 = Cpf.Criar("52998224725");

        Assert.True(cpf1.Equals(cpf2));
        Assert.Equal(cpf1.GetHashCode(), cpf2.GetHashCode());
    }

    [Fact]
    public void Cpf_Equals_DeveRetornarFalse_QuandoValoresDiferentes()
    {
        var cpf = Cpf.Criar("529.982.247-25");
        var outroCpf = default(Cpf);

        Assert.False(cpf.Equals(outroCpf));
    }

    [Fact]
    public void Cpf_Equals_DeveRetornarFalse_QuandoOutroTipo()
    {
        var cpf = Cpf.Criar("529.982.247-25");

        Assert.False(cpf.Equals("52998224725"));
    }

    [Fact]
    public void Cpf_ToString_DeveRetornarValorNormalizado()
    {
        var cpf = Cpf.Criar("529.982.247-25");

        Assert.Equal("52998224725", cpf.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cnpj_Criar_DeveLancarQuandoVazio(string valor)
    {
        var exception = Assert.Throws<DomainException>(() => { Cnpj.Criar(valor); });
        Assert.Equal("A identificação do cliente é obrigatória.", exception.Message);
    }

    [Fact]
    public void Cnpj_EhValido_DeveRetornarFalse_QuandoVazio()
    {
        Assert.False(Cnpj.EhValido(""));
        Assert.False(Cnpj.EhValido("   "));
    }

    [Fact]
    public void Cnpj_EhValido_DeveRetornarFalse_QuandoInvalido()
    {
        Assert.False(Cnpj.EhValido("11.111.111/1111-11"));
        Assert.False(Cnpj.EhValido("123"));
    }

    [Fact]
    public void Cnpj_Equals_DeveRetornarTrue_QuandoMesmoValor()
    {
        var cnpj1 = Cnpj.Criar("04.252.011/0001-10");
        var cnpj2 = Cnpj.Criar("04252011000110");

        Assert.True(cnpj1.Equals(cnpj2));
        Assert.Equal(cnpj1.GetHashCode(), cnpj2.GetHashCode());
    }

    [Fact]
    public void Cnpj_Equals_DeveRetornarFalse_QuandoValoresDiferentes()
    {
        var cnpj1 = Cnpj.Criar("04.252.011/0001-10");
        var cnpj2 = Cnpj.Criar("11.222.333/0001-81");

        Assert.False(cnpj1.Equals(cnpj2));
    }

    [Fact]
    public void Cnpj_Equals_DeveRetornarFalse_QuandoOutroTipo()
    {
        var cnpj = Cnpj.Criar("04.252.011/0001-10");

        Assert.False(cnpj.Equals("04252011000110"));
    }

    [Fact]
    public void Cnpj_ToString_DeveRetornarValorNormalizado()
    {
        var cnpj = Cnpj.Criar("04.252.011/0001-10");

        Assert.Equal("04252011000110", cnpj.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IdentificacaoCliente_Criar_DeveLancarQuandoVazio(string valor)
    {
        var action = () => IdentificacaoCliente.Criar(valor);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("A identificação do cliente é obrigatória.", exception.Message);
    }

    [Fact]
    public void IdentificacaoCliente_ToString_DeveRetornarValor()
    {
        var identificacao = IdentificacaoCliente.Criar("52998224725");

        Assert.Equal("52998224725", identificacao.ToString());
    }

    [Fact]
    public void IdentificacaoCliente_Equals_DeveRetornarTrue_QuandoMesmoValorETipo()
    {
        var id1 = IdentificacaoCliente.Criar("52998224725");
        var id2 = IdentificacaoCliente.Criar("52998224725");

        Assert.True(id1.Equals(id2));
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void IdentificacaoCliente_Equals_DeveRetornarFalse_QuandoValoresDiferentes()
    {
        var id1 = IdentificacaoCliente.Criar("52998224725");
        var id2 = IdentificacaoCliente.Criar("04.252.011/0001-10");

        Assert.False(id1.Equals(id2));
    }

    [Fact]
    public void IdentificacaoCliente_Equals_DeveRetornarFalse_QuandoNull()
    {
        var id = IdentificacaoCliente.Criar("52998224725");

        Assert.False(id.Equals(null));
        Assert.False(id.Equals((object?)null));
    }
}
