using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.OrdensServico;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests.Features.OrdensServico;

public sealed class OrdemServicoGatewayTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirOrdemServicoComServicosEHistorico()
    {
        await using var context = CriarContexto();
        var cliente = await AdicionarClienteAsync(context);
        var veiculo = await AdicionarVeiculoAsync(context, cliente.Id);
        var servico = await AdicionarServicoAsync(context, "Revisao");
        var repository = new OrdemServicoGateway(context);
        var ordemServico = OrdemServico.Criar(cliente.Id, veiculo.Id, [servico]);

        await repository.AdicionarAsync(ordemServico);

        var salvo = await context.OrdensServico.Include(o => o.Servicos).Include(o => o.HistoricoStatus).FirstAsync();
        Assert.Single(salvo.Servicos);
        Assert.Single(salvo.HistoricoStatus);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarOrdemServico_QuandoExiste()
    {
        await using var context = CriarContexto();
        var cliente = await AdicionarClienteAsync(context);
        var veiculo = await AdicionarVeiculoAsync(context, cliente.Id);
        var servico = await AdicionarServicoAsync(context, "Revisao");
        var repository = new OrdemServicoGateway(context);
        var ordemServico = OrdemServico.Criar(cliente.Id, veiculo.Id, [servico]);
        await repository.AdicionarAsync(ordemServico);

        var encontrado = await repository.ObterPorIdAsync(ordemServico.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(ordemServico.Id, encontrado.Id);
        Assert.Single(encontrado.Servicos);
        Assert.Single(encontrado.HistoricoStatus);
    }

    [Fact]
    public async Task ListarPorClienteAsync_DeveRetornarSomenteOrdensDoClienteInformado()
    {
        await using var context = CriarContexto();
        var clienteA = await AdicionarClienteAsync(context, "52998224725");
        var clienteB = await AdicionarClienteAsync(context, "04252011000110");
        var veiculoA = await AdicionarVeiculoAsync(context, clienteA.Id, "ABC1D23");
        var veiculoB = await AdicionarVeiculoAsync(context, clienteB.Id, "XYZ9A00");
        var servico = await AdicionarServicoAsync(context, "Revisao");
        var repository = new OrdemServicoGateway(context);

        await repository.AdicionarAsync(OrdemServico.Criar(clienteA.Id, veiculoA.Id, [servico]));
        await repository.AdicionarAsync(OrdemServico.Criar(clienteB.Id, veiculoB.Id, [servico]));

        var ordens = await repository.ListarPorClienteAsync(clienteA.Id);

        Assert.Single(ordens);
        Assert.Equal(clienteA.Id, ordens.First().ClienteId);
    }

    [Fact]
    public async Task ListarPorStatusAsync_DeveRetornarSomenteOrdensNoStatusInformado()
    {
        await using var context = CriarContexto();
        var cliente = await AdicionarClienteAsync(context);
        var veiculo = await AdicionarVeiculoAsync(context, cliente.Id);
        var servico = await AdicionarServicoAsync(context, "Revisao");
        var repository = new OrdemServicoGateway(context);
        var ordemServico = OrdemServico.Criar(cliente.Id, veiculo.Id, [servico], new DateTime(2024, 01, 01, 8, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEmDiagnostico(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEmExecucao(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaFinalizada(new DateTime(2024, 01, 01, 11, 00, 00, DateTimeKind.Utc));
        ordemServico.AlterarParaEntregue(new DateTime(2024, 01, 01, 12, 00, 00, DateTimeKind.Utc));

        await repository.AdicionarAsync(ordemServico);

        var ordens = await repository.ListarPorStatusAsync(StatusOrdemServico.Entregue);

        Assert.Single(ordens);
        Assert.Equal(StatusOrdemServico.Entregue, ordens.First().Status);
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirOrdemServico()
    {
        await using var context = CriarContexto();
        var cliente = await AdicionarClienteAsync(context);
        var veiculo = await AdicionarVeiculoAsync(context, cliente.Id);
        var servico = await AdicionarServicoAsync(context, "Revisao");
        var repository = new OrdemServicoGateway(context);
        var ordemServico = OrdemServico.Criar(cliente.Id, veiculo.Id, [servico]);
        await repository.AdicionarAsync(ordemServico);

        await repository.RemoverAsync(ordemServico);

        Assert.Empty(await context.OrdensServico.ToListAsync());
    }

    [Fact]
    public async Task AtualizarAsync_DevePersistirSomenteUltimoOrcamentoDaOrdem()
    {
        await using var context = CriarContexto();
        var cliente = await AdicionarClienteAsync(context);
        var veiculo = await AdicionarVeiculoAsync(context, cliente.Id);
        var insumo = await AdicionarInsumoAsync(context, "Oleo", 25m);
        var servico = await AdicionarServicoComItemAsync(context, "Troca", insumo, 2);
        var repository = new OrdemServicoGateway(context);
        var ordemServico = OrdemServico.Criar(cliente.Id, veiculo.Id, [servico]);

        await repository.AdicionarAsync(ordemServico);

        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 9, 00, 00, DateTimeKind.Utc));
        await repository.AtualizarAsync(ordemServico);

        insumo.AtualizarValorUnitario(40m);
        await context.SaveChangesAsync();

        ordemServico.GerarOrcamento(new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc));
        await repository.AtualizarAsync(ordemServico);

        var salvo = await repository.ObterPorIdAsync(ordemServico.Id);

        Assert.NotNull(salvo);
        Assert.NotNull(salvo!.Orcamento);
        Assert.Equal(80m, salvo.Orcamento!.ValorTotal);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, salvo.Status);
        Assert.Single(salvo.Orcamento.Servicos);
    }

    private static OficinaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OficinaDbContext(options);
    }

    private static async Task<Cliente> AdicionarClienteAsync(OficinaDbContext context, string identificacao = "52998224725")
    {
        var cliente = Cliente.Criar("Cliente Teste", IdentificacaoCliente.Criar(identificacao));
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    private static async Task<Veiculo> AdicionarVeiculoAsync(OficinaDbContext context, Guid clienteId, string placa = "ABC1D23")
    {
        var veiculo = Veiculo.Criar(placa, "Toyota", "Corolla", 2023, placa == "ABC1D23" ? "12345678901" : "10987654321", clienteId);
        await context.Veiculos.AddAsync(veiculo);
        await context.SaveChangesAsync();
        return veiculo;
    }

    private static async Task<Servico> AdicionarServicoAsync(OficinaDbContext context, string nome)
    {
        var servico = Servico.Criar(nome, "Servico completo", []);
        await context.Servicos.AddAsync(servico);
        await context.SaveChangesAsync();
        return servico;
    }

    private static async Task<Insumo> AdicionarInsumoAsync(OficinaDbContext context, string nome, decimal valorUnitario)
    {
        var insumo = Insumo.Criar(nome, "Fabricante", 20, valorUnitario);
        await context.Insumos.AddAsync(insumo);
        await context.SaveChangesAsync();
        return insumo;
    }

    private static async Task<Servico> AdicionarServicoComItemAsync(OficinaDbContext context, string nome, Insumo insumo, int quantidade)
    {
        var servico = Servico.Criar(nome, "Servico completo", [ItemServico.Criar(insumo, quantidade)]);
        await context.Servicos.AddAsync(servico);
        await context.SaveChangesAsync();
        return servico;
    }
}
