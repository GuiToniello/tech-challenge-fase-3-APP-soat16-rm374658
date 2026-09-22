using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Servicos;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests.Features.Servicos;

public sealed class ServicoGatewayTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirServicoComItensServico()
    {
        await using var context = CriarContexto();
        var repository = new ServicoGateway(context);
        var insumo = await AdicionarInsumoAsync(context, "Filtro de Oleo");
        var servico = Servico.Criar("Troca de Oleo", "Troca completa", [ItemServico.Criar(insumo, 2)]);

        await repository.AdicionarAsync(servico);

        var salvo = await context.Servicos
            .Include(s => s.ItensServico)
            .ThenInclude(itemServico => itemServico.Insumo)
            .FirstAsync();

        Assert.Single(salvo.ItensServico);
        Assert.Equal(2, salvo.ItensServico.Single().Quantidade);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosDoServico()
    {
        await using var context = CriarContexto();
        var repository = new ServicoGateway(context);
        var insumo = await AdicionarInsumoAsync(context, "Filtro de Oleo");
        var servico = Servico.Criar("Troca de Oleo", "Troca completa", [ItemServico.Criar(insumo, 1)]);

        await repository.AdicionarAsync(servico);
        servico.AtualizarNome("Alinhamento");

        await repository.AtualizarAsync(servico);

        var salvo = await context.Servicos.FirstAsync(s => s.Id == servico.Id);
        Assert.Equal("Alinhamento", salvo.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new ServicoGateway(context);

        var encontrado = await repository.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarOrdenadoPorNome()
    {
        await using var context = CriarContexto();
        var repository = new ServicoGateway(context);

        await repository.AdicionarAsync(Servico.Criar("Troca", "Servico", []));
        await repository.AdicionarAsync(Servico.Criar("Alinhamento", "Servico", []));

        var servicos = await repository.ListarAsync();

        Assert.Equal(new[] { "Alinhamento", "Troca" }, servicos.Select(s => s.Nome));
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirServico()
    {
        await using var context = CriarContexto();
        var repository = new ServicoGateway(context);
        var servico = Servico.Criar("Troca", "Servico", []);

        await repository.AdicionarAsync(servico);
        await repository.RemoverAsync(servico);

        Assert.Empty(await context.Servicos.ToListAsync());
    }

    private static OficinaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OficinaDbContext(options);
    }

    private static async Task<Insumo> AdicionarInsumoAsync(OficinaDbContext context, string nome)
    {
        var insumo = Insumo.Criar(nome, "Bosch", 10, 19.90m);
        await context.Insumos.AddAsync(insumo);
        await context.SaveChangesAsync();
        return insumo;
    }
}
