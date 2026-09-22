using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Veiculos;
using Xunit;

namespace TechChallenge.Oficina.DB.Data.Tests.Features.Veiculos;

public sealed class VeiculoGatewayTests
{
    [Fact]
    public async Task AdicionarAsync_DevePersistirVeiculo()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);

        Assert.Equal(1, await context.Veiculos.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosDoVeiculo()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);
        veiculo.AtualizarMarca("Honda");
        await repository.AtualizarAsync(veiculo);

        var salvo = await context.Veiculos.FirstAsync(v => v.Id == veiculo.Id);
        Assert.Equal("Honda", salvo.Marca);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new VeiculoGateway(context);

        var veiculo = await repository.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(veiculo);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarVeiculo_QuandoExiste()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);

        var encontrado = await repository.ObterPorIdAsync(veiculo.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(veiculo.Id, encontrado.Id);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarOrdenadoPorMarcaEModelo()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;

        await repository.AdicionarAsync(CriarVeiculo("XYZ9A00", clienteId, "Toyota", "Corolla"));
        await repository.AdicionarAsync(CriarVeiculo("ABC1D23", clienteId, "Honda", "Civic"));

        var veiculos = await repository.ListarAsync();

        Assert.Equal(new[] { "Honda", "Toyota" }, veiculos.Select(v => v.Marca));
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVazia_QuandoSemVeiculos()
    {
        await using var context = CriarContexto();
        var repository = new VeiculoGateway(context);

        var veiculos = await repository.ListarAsync();

        Assert.Empty(veiculos);
    }

    [Fact]
    public async Task ListarPorClienteAsync_DeveRetornarSomenteVeiculosDoCliente()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context, "52998224725");
        await AdicionarClienteAsync(context, "04252011000110");
        var repository = new VeiculoGateway(context);

        var clientes = context.Clientes.ToList();
        var clienteA = clientes[0];
        var clienteB = clientes[1];

        await repository.AdicionarAsync(CriarVeiculo("ABC1D23", clienteA.Id));
        await repository.AdicionarAsync(CriarVeiculo("XYZ9A00", clienteB.Id));

        var veiculosDoA = await repository.ListarPorClienteAsync(clienteA.Id);

        Assert.Single(veiculosDoA);
        Assert.Equal(clienteA.Id, veiculosDoA.First().ClienteId);
    }

    [Fact]
    public async Task ExisteComPlacaAsync_DeveRetornarTrue_QuandoExiste()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);

        var existe = await repository.ExisteComPlacaAsync("ABC1D23");

        Assert.True(existe);
    }

    [Fact]
    public async Task ExisteComPlacaAsync_DeveRetornarFalse_QuandoNaoExiste()
    {
        await using var context = CriarContexto();
        var repository = new VeiculoGateway(context);

        var existe = await repository.ExisteComPlacaAsync("ABC1D23");

        Assert.False(existe);
    }

    [Fact]
    public async Task ExisteComPlacaAsync_DeveRespeitarVeiculoIgnorado()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);

        var existeSemIgnorar = await repository.ExisteComPlacaAsync("ABC1D23");
        var existeIgnorando = await repository.ExisteComPlacaAsync("ABC1D23", veiculo.Id);

        Assert.True(existeSemIgnorar);
        Assert.False(existeIgnorando);
    }

    [Fact]
    public async Task RemoverAsync_DeveExcluirVeiculo()
    {
        await using var context = CriarContexto();
        await AdicionarClienteAsync(context);
        var repository = new VeiculoGateway(context);
        var clienteId = context.Clientes.First().Id;
        var veiculo = CriarVeiculo("ABC1D23", clienteId);

        await repository.AdicionarAsync(veiculo);
        await repository.RemoverAsync(veiculo);

        Assert.Empty(await context.Veiculos.ToListAsync());
    }

    private static OficinaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OficinaDbContext(options);
    }

    private static async Task AdicionarClienteAsync(OficinaDbContext context, string identificacao = "52998224725")
    {
        var cliente = Cliente.Criar("Cliente Teste", IdentificacaoCliente.Criar(identificacao));
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();
    }

    private static Veiculo CriarVeiculo(string placa, Guid clienteId, string marca = "Toyota", string modelo = "Corolla")
    {
        return Veiculo.Criar(placa, marca, modelo, 2023, "12345678901", clienteId);
    }
}
