using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.GetOSService.API.Tests.Infrastructure;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
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

namespace TechChallenge.Oficina.GetOSService.API.Tests.Features.OrdensServico;

public sealed class GetOrdensServicoEndpointsTests : IDisposable
{
    private readonly GetOSServiceIntegrationTestFixture _fixture;
    private readonly IntegrationTestFixture _fixtureComCompleta; // Para criar dados base
    private int _clienteCpfIndex;

    public GetOrdensServicoEndpointsTests()
    {
        _fixture = new GetOSServiceIntegrationTestFixture();
        _fixtureComCompleta = new IntegrationTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
        _fixtureComCompleta.Dispose();
    }

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixtureComCompleta.CriarClientesEndpoints();
        var cpf = _clienteCpfIndex++ == 0 ? "529.982.247-25" : $"123.456.789-{_clienteCpfIndex:02d}";
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente GetOS", Identificacao = cpf, Email = $"getos{cpf[..3]}@email.com" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixtureComCompleta.CriarVeiculosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(
            new CriarVeiculoCommand { Placa = "XYZ9K99", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901", ClienteId = clienteId },
            CancellationToken.None));
        return result.Value.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixtureComCompleta.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand { Nome = "Oleo Motor", Fabricante = "Castrol", QuantidadeDisponivel = 50, ValorUnitario = 25m },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixtureComCompleta.CriarServicosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Oleo",
                Descricao = "Troca de oleo do motor",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 2 }]
            },
            CancellationToken.None));
        return result.Value.Id;
    }

    private async Task<(Guid clienteId, Guid veiculoId, Guid servicoId)> CriarContextoCompletoAsync()
    {
        var clienteId = await CriarClienteComEmailAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);
        return (clienteId, veiculoId, servicoId);
    }

    [Fact]
    public async Task GetOrdenadas_SemOrdensAtivas_DeveRetornarListaVazia()
    {
        // Arrange
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        // Act
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        // Assert
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetOrdenadas_ComUmaOrdemServico_DeveRetornarUmaOrdem()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixtureComCompleta.CriarOrdensServicoEndpoints();
        
        var ordem = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        // Act
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        // Assert
        Assert.Single(result.Value);
        var ordemRetornada = result.Value.First();
        Assert.Equal(ordem.Id, ordemRetornada.Id);
        Assert.NotEqual(default, ordemRetornada.DataAlteracao);
    }

    [Fact]
    public async Task GetOrdenadas_ComMultiplasOrdensComDiferentesStatus_DeveRetornarOrdenadasPorStatusEData()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixtureComCompleta.CriarOrdensServicoEndpoints();

        // Criar múltiplas ordens com diferentes status
        var ordem1 = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        var ordem2 = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        var ordem3 = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        var ordem4 = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        // Alterar statuses para criar diferentes estados
        await endpoints.AlterarParaEmDiagnostico(ordem2.Id, CancellationToken.None);

        await endpoints.AlterarParaEmDiagnostico(ordem3.Id, CancellationToken.None);
        await endpoints.GerarOrcamento(ordem3.Id, CancellationToken.None);

        await endpoints.AlterarParaEmDiagnostico(ordem4.Id, CancellationToken.None);
        await endpoints.GerarOrcamento(ordem4.Id, CancellationToken.None);
        await endpoints.AlterarParaEmExecucao(ordem4.Id, CancellationToken.None);

        // Act
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        var ordensOrdenadas = result.Value.ToList();

        // Assert
        Assert.NotEmpty(ordensOrdenadas);
        Assert.Equal(4, ordensOrdenadas.Count);

        // Primeira ordem deve ser a que está em execução (ordem4)
        Assert.Equal(ordem4.Id, ordensOrdenadas[0].Id);
        Assert.Equal((int)TechChallenge.Oficina.Entities.Features.OrdensServico.Enums.StatusOrdemServico.EmExecucao, 
            ordensOrdenadas[0].Status);
        
        // Todas devem ter DataAlteracao preenchida
        Assert.All(ordensOrdenadas, o => Assert.NotEqual(default, o.DataAlteracao));
    }

    [Fact]
    public async Task GetOrdenadas_ComOrdensFinalizadaEEntregue_DeveExcluirEssasOrdensDoResultado()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixtureComCompleta.CriarOrdensServicoEndpoints();

        // Criar ordem e deixar em estado recebido
        var ordemRecebida = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        // Criar ordem e finalizar
        var ordemFinalizada = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        await endpoints.AlterarParaEmDiagnostico(ordemFinalizada.Id, CancellationToken.None);
        await endpoints.AlterarParaEmExecucao(ordemFinalizada.Id, CancellationToken.None);
        await endpoints.AlterarParaFinalizada(ordemFinalizada.Id, CancellationToken.None);

        // Criar ordem e entregar
        var ordemEntregue = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        await endpoints.AlterarParaEmDiagnostico(ordemEntregue.Id, CancellationToken.None);
        await endpoints.AlterarParaEmExecucao(ordemEntregue.Id, CancellationToken.None);
        await endpoints.AlterarParaFinalizada(ordemEntregue.Id, CancellationToken.None);
        await endpoints.AlterarParaEntregue(ordemEntregue.Id, CancellationToken.None);

        // Act
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        var ordensOrdenadas = result.Value.ToList();

        // Assert
        // Deve conter apenas a ordem recebida, não as finalizadas ou entregues
        Assert.Single(ordensOrdenadas);
        Assert.Equal(ordemRecebida.Id, ordensOrdenadas[0].Id);

        var ordemFinalizadaRetornada = ordensOrdenadas.FirstOrDefault(o => o.Id == ordemFinalizada.Id);
        Assert.Null(ordemFinalizadaRetornada);

        var ordemEntregueRetornada = ordensOrdenadas.FirstOrDefault(o => o.Id == ordemEntregue.Id);
        Assert.Null(ordemEntregueRetornada);
    }

    [Fact]
    public async Task GetOrdenadas_DeveRetornarOrdensComDataAlteracao()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixtureComCompleta.CriarOrdensServicoEndpoints();

        var ordem = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Value!;

        await endpoints.AlterarParaEmDiagnostico(ordem.Id, CancellationToken.None);

        // Act
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        var ordensOrdenadas = result.Value.ToList();

        // Assert
        Assert.Single(ordensOrdenadas);
        var ordemRetornada = ordensOrdenadas[0];
        Assert.NotEqual(default, ordemRetornada.DataAlteracao);
        Assert.True(ordemRetornada.DataAlteracao > DateTime.MinValue, "DataAlteracao deve ser uma data válida");
    }
}
