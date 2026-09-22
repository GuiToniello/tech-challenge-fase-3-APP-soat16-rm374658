using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Insumos;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests.Features.Insumos;

public sealed class InsumoGatewayTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirInsumo()
    {
        await using var context = CriarContexto();
        var repository = new InsumoGateway(context);
        var insumo = CriarInsumo("Óleo", "Bosch", 10, 21.9m);

        await repository.AdicionarAsync(insumo);

        Assert.Equal(1, await context.Insumos.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosDoInsumo()
    {
        await using var context = CriarContexto();
        var repository = new InsumoGateway(context);
        var insumo = CriarInsumo("Óleo", "Bosch", 10, 21.9m);

        await repository.AdicionarAsync(insumo);
        insumo.AtualizarNome("Filtro de Ar");

        await repository.AtualizarAsync(insumo);

        var salvo = await context.Insumos.FirstAsync(i => i.Id == insumo.Id);
        Assert.Equal("Filtro de Ar", salvo.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new InsumoGateway(context);

        var encontrado = await repository.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarOrdenadoPorNomeEFabricante()
    {
        await using var context = CriarContexto();
        var repository = new InsumoGateway(context);

        await repository.AdicionarAsync(CriarInsumo("Óleo", "Y", 10, 21.9m));
        await repository.AdicionarAsync(CriarInsumo("Óleo", "A", 8, 25.9m));

        var insumos = await repository.ListarAsync();

        Assert.Equal(new[] { "A", "Y" }, insumos.Select(i => i.Fabricante));
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirInsumo()
    {
        await using var context = CriarContexto();
        var repository = new InsumoGateway(context);
        var insumo = CriarInsumo("Óleo", "Bosch", 10, 21.9m);

        await repository.AdicionarAsync(insumo);
        await repository.RemoverAsync(insumo);

        Assert.Empty(await context.Insumos.ToListAsync());
    }

    private static OficinaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OficinaDbContext(options);
    }

    private static Insumo CriarInsumo(string nome, string fabricante, int quantidade, decimal valor)
    {
        return Insumo.Criar(nome, fabricante, quantidade, valor);
    }
}
