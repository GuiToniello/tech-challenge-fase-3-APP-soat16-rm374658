using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests;

public sealed class FalhasBancoTests
{
    [Fact]
    public void EhIndisponibilidade_DeveRetornarTrue_QuandoNpgsqlExceptionEstaEmbrulhadaEmInvalidOperationException()
    {
        var excecao = new InvalidOperationException(
            "Falha transitória",
            new NpgsqlException("Falha ao conectar no banco", new SocketException((int)SocketError.ConnectionRefused)));

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.True(indisponivel);
    }

    [Fact]
    public void EhIndisponibilidade_DeveRetornarFalse_QuandoDbUpdateExceptionTemViolacaoDeRestricao()
    {
        var excecao = new DbUpdateException(
            "Erro ao salvar",
            new PostgresException("Violação de chave estrangeira", "ERROR", "ERROR", "23503"));

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.False(indisponivel);
    }

    [Fact]
    public void EhIndisponibilidade_DeveRetornarTrue_QuandoTimeoutException()
    {
        var excecao = new TimeoutException("Timeout na operação");

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.True(indisponivel);
    }

    [Fact]
    public void EhIndisponibilidade_DeveRetornarFalse_QuandoTaskCanceledExceptionEmbrulhaTimeoutException()
    {
        var excecao = new TaskCanceledException("Timeout do HttpClient no envio de email", new TimeoutException("Tempo esgotado"));

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.False(indisponivel);
    }

    [Fact]
    public void EhIndisponibilidade_DeveRetornarFalse_QuandoOperationCanceledExceptionEmbrulhaTimeoutException()
    {
        var excecao = new OperationCanceledException("Consulta cancelada pela requisicao", new TimeoutException("Tempo esgotado"));

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.False(indisponivel);
    }

    [Fact]
    public void EhIndisponibilidade_DeveRetornarFalse_QuandoInvalidOperationExceptionNaoTemCausaDeBanco()
    {
        var excecao = new InvalidOperationException("Erro inesperado");

        var indisponivel = FalhasBanco.EhIndisponibilidade(excecao);

        Assert.False(indisponivel);
    }
}
