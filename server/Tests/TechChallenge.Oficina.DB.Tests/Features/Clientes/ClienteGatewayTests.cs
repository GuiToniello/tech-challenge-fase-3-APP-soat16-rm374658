using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Clientes;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests.Features.Clientes;

public sealed class ClienteGatewayTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirCliente()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = CriarCliente("Cliente A", "52998224725");

        await repository.AdicionarAsync(cliente);

        Assert.Equal(1, await context.Clientes.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosDoCliente()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = CriarCliente("Nome Antigo", "52998224725");

        await repository.AdicionarAsync(cliente);
        cliente.AtualizarNomeCompleto("Nome Novo");

        await repository.AtualizarAsync(cliente);

        var salvo = await context.Clientes.FirstAsync(c => c.Id == cliente.Id);
        Assert.Equal("Nome Novo", salvo.NomeCompleto);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);

        var cliente = await repository.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(cliente);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarOrdenadoPorNome()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);

        await repository.AdicionarAsync(CriarCliente("Z Cliente", "04252011000110"));
        await repository.AdicionarAsync(CriarCliente("A Cliente", "52998224725"));

        var clientes = await repository.ListarAsync();

        Assert.Equal(new[] { "A Cliente", "Z Cliente" }, clientes.Select(c => c.NomeCompleto));
    }

    [Fact]
    public async Task ExisteComIdentificacaoAsync_DeveRespeitarClienteIgnorado()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = CriarCliente("Cliente", "52998224725");

        await repository.AdicionarAsync(cliente);

        var existeSemIgnorar = await repository.ExisteComIdentificacaoAsync("52998224725");
        var existeIgnorandoMesmoId = await repository.ExisteComIdentificacaoAsync("52998224725", cliente.Id);

        Assert.True(existeSemIgnorar);
        Assert.False(existeIgnorandoMesmoId);
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirCliente()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = CriarCliente("Cliente", "52998224725");

        await repository.AdicionarAsync(cliente);
        await repository.RemoverAsync(cliente);

        Assert.Empty(await context.Clientes.ToListAsync());
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarCliente_QuandoExiste()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = CriarCliente("Cliente A", "52998224725");

        await repository.AdicionarAsync(cliente);

        var encontrado = await repository.ObterPorIdAsync(cliente.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(cliente.Id, encontrado.Id);
        Assert.Equal("Cliente A", encontrado.NomeCompleto);
    }

    [Fact]
    public async Task ExisteComIdentificacaoAsync_DeveRetornarFalse_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);

        var existe = await repository.ExisteComIdentificacaoAsync("52998224725");

        Assert.False(existe);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVazia_QuandoSemClientes()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);

        var clientes = await repository.ListarAsync();

        Assert.Empty(clientes);
    }

    [Fact]
    public async Task AdicionarAsync_DevePersistirMultiplosClientes()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);

        await repository.AdicionarAsync(CriarCliente("Cliente B", "52998224725"));
        await repository.AdicionarAsync(CriarCliente("Cliente A", "04252011000110"));

        Assert.Equal(2, await context.Clientes.CountAsync());
    }

    [Fact]
    public async Task AdicionarAsync_DevePersistirEmailQuandoInformado()
    {
        await using var context = CriarContexto();
        var repository = new ClienteGateway(context);
        var cliente = Cliente.Criar("Cliente Email", IdentificacaoCliente.Criar("52998224725"), "cliente@teste.com");

        await repository.AdicionarAsync(cliente);

        var salvo = await context.Clientes.FirstAsync(c => c.Id == cliente.Id);
        Assert.Equal("cliente@teste.com", salvo.Email);
    }

    private static OficinaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OficinaDbContext(options);
    }

    private static Cliente CriarCliente(string nome, string identificacao)
    {
        return Cliente.Criar(nome, IdentificacaoCliente.Criar(identificacao));
    }
}
