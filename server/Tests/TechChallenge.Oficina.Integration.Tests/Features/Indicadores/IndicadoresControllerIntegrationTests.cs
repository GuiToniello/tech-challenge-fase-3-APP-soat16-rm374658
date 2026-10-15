using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;
using Xunit;

namespace TechChallenge.Oficina.Integration.Tests.Features.Indicadores;

public sealed class IndicadoresControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public IndicadoresControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Indicadores", Identificacao = "529.982.247-25", Email = "indicadores@email.com" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(
            new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901", ClienteId = clienteId },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<InsumoViewModel>>(await endpoints.Post(
            new CriarInsumoCommand { Nome = "Filtro de Oleo", Fabricante = "Tecfil", QuantidadeDisponivel = 20, ValorUnitario = 30m },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Filtro",
                Descricao = "Substituicao do filtro de oleo",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
            },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<(Guid clienteId, Guid veiculoId, Guid servicoId)> CriarContextoCompletoAsync()
    {
        var clienteId = await CriarClienteComEmailAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);
        return (clienteId, veiculoId, servicoId);
    }

    private async Task<Guid> CriarOrdemServicoAsync(Guid clienteId, Guid veiculoId, Guid servicoId)
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        return created.Value!.Id;
    }

    private async Task<Guid> CriarOrdemComOrcamentoRecusadoAsync(Guid clienteId, Guid veiculoId, Guid servicoId)
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var ordemId = await CriarOrdemServicoAsync(clienteId, veiculoId, servicoId);

        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.GerarOrcamento(ordemId, CancellationToken.None));
        var recusada = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.RecusarOrcamento(ordemId, CancellationToken.None));
        Assert.Equal(5, recusada.Value!.Status); // Finalizada = 5, sem passar por EmExecucao

        return ordemId;
    }

    [Fact]
    public async Task AlterarParaEntregue_OrdemComOrcamentoRecusado_DeveRetornar200EAtualizarIndicadores()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var ordemRecusadaId = await CriarOrdemComOrcamentoRecusadoAsync(clienteId, veiculoId, servicoId);
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        var entregue = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEntregue(ordemRecusadaId, CancellationToken.None));
        Assert.Equal(6, entregue.Value!.Status); // Entregue = 6

        var indicadores = Assert.IsType<Ok<IndicadorViewModel>>(await _fixture.CriarIndicadoresEndpoints().Get(CancellationToken.None));
        Assert.True(indicadores.Value!.TempoMedioEntrega > TimeSpan.Zero);
        Assert.Equal(TimeSpan.Zero, indicadores.Value.TempoMedioExecucao);
    }

    [Fact]
    public async Task Get_AposEntregarOrdemRecusadaEOrdemExecutada_DeveRetornar200ComTemposMedios()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var ordemRecusadaId = await CriarOrdemComOrcamentoRecusadoAsync(clienteId, veiculoId, servicoId);
        var ordemExecutadaId = await CriarOrdemServicoAsync(clienteId, veiculoId, servicoId);
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEntregue(ordemRecusadaId, CancellationToken.None));

        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEmDiagnostico(ordemExecutadaId, CancellationToken.None));
        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.GerarOrcamento(ordemExecutadaId, CancellationToken.None));
        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AprovarOrcamento(ordemExecutadaId, CancellationToken.None));
        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaFinalizada(ordemExecutadaId, CancellationToken.None));
        var entregue = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEntregue(ordemExecutadaId, CancellationToken.None));
        Assert.Equal(6, entregue.Value!.Status); // Entregue = 6

        var indicadores = Assert.IsType<Ok<IndicadorViewModel>>(await _fixture.CriarIndicadoresEndpoints().Get(CancellationToken.None));
        Assert.True(indicadores.Value!.TempoMedioEntrega > TimeSpan.Zero);
        Assert.True(indicadores.Value.TempoMedioExecucao > TimeSpan.Zero);
    }
}
