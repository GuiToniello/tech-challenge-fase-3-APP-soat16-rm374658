using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;
using Xunit;

namespace TechChallenge.Oficina.Integration.Tests.Features.OrdensServico;

public sealed class OrdemServicoStatusNotificacaoIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;
    private readonly Mock<IOrdemServicoStatusEmailSender> _emailSenderMock = new();
    private int _clienteCpfIndex;

    public OrdemServicoStatusNotificacaoIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var cpf = _clienteCpfIndex++ == 0 ? "529.982.247-25" : "123.456.789-09";
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand 
            { 
                NomeCompleto = "Cliente Notificacao", 
                Identificacao = cpf, 
                Email = $"notif{cpf[..3]}@email.com" 
            },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(
            new CriarVeiculoCommand 
            { 
                Placa = "XYZ9W87", 
                Marca = "Toyota", 
                Modelo = "Corolla", 
                Ano = 2023, 
                Renavam = "98765432109", 
                ClienteId = clienteId 
            },
            CancellationToken.None));
        return result.Value.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand 
            { 
                Nome = "Oleo Sintetico", 
                Fabricante = "Shell", 
                QuantidadeDisponivel = 50, 
                ValorUnitario = 45m 
            },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Oleo",
                Descricao = "Troca de oleo do motor",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
            },
            CancellationToken.None));
        return result.Value.Id;
    }

    private async Task<Guid> CriarOrdemServicoAsync(Guid clienteId, Guid veiculoId, Guid servicoId)
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand 
        { 
            ClienteId = clienteId, 
            VeiculoId = veiculoId, 
            ServicoIds = [servicoId] 
        };

        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(
            await endpoints.Post(command, CancellationToken.None));
        return created.Value!.Id;
    }

    private async Task<(Guid clienteId, Guid veiculoId, Guid servicoId, Guid ordemId)> CriarContextoCompletoAsync()
    {
        var clienteId = await CriarClienteComEmailAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);
        var ordemId = await CriarOrdemServicoAsync(clienteId, veiculoId, servicoId);
        return (clienteId, veiculoId, servicoId, ordemId);
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_OrdemRecebida_DeveRetornar200ComStatusAlterado()
    {
        var (_, _, _, ordemId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        var result = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None));

        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Status); // EmDiagnostico = 2
    }

    [Fact]
    public async Task AlterarParaEmExecucao_OrdemEmDiagnostico_DeveRetornar200ComStatusAlterado()
    {
        var (_, _, _, ordemId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        // Primeiro altera para Em Diagnóstico
        await endpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None);

        // Depois para Em Execução
        var result = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEmExecucao(ordemId, CancellationToken.None));

        Assert.NotNull(result.Value);
        Assert.Equal(4, result.Value!.Status); // EmExecucao = 4
    }

    [Fact]
    public async Task AlterarParaFinalizada_OrdemEmExecucao_DeveRetornar200ComStatusAlterado()
    {
        var (_, _, _, ordemId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        // Primeiro altera para Em Diagnóstico
        await endpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None);

        // Depois para Em Execução
        await endpoints.AlterarParaEmExecucao(ordemId, CancellationToken.None);

        // Depois para Finalizada
        var result = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaFinalizada(ordemId, CancellationToken.None));

        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value!.Status); // Finalizada = 5
    }

    [Fact]
    public async Task AlterarParaEntregue_OrdemFinalizada_DeveRetornar200ComStatusAlterado()
    {
        var (_, _, _, ordemId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        // Primeiro altera para Em Diagnóstico
        await endpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None);

        // Depois para Em Execução
        await endpoints.AlterarParaEmExecucao(ordemId, CancellationToken.None);

        // Depois para Finalizada
        await endpoints.AlterarParaFinalizada(ordemId, CancellationToken.None);

        // Depois para Entregue
        var result = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEntregue(ordemId, CancellationToken.None));

        Assert.NotNull(result.Value);
        Assert.Equal(6, result.Value!.Status); // Entregue = 6
    }

    [Fact]
    public async Task FluxoDeTransicoes_DeveAlterarStatusCorreamentePorCompleto()
    {
        var (_, _, _, ordemId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        // Estado inicial: Recebida
        var getInicial = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.GetById(ordemId, CancellationToken.None));
        Assert.Equal(1, getInicial.Value!.Status); // Recebida = 1

        // Transição 1: Recebida -> Em Diagnóstico
        var alterado1 = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None));
        Assert.Equal(2, alterado1.Value!.Status); // EmDiagnostico = 2

        // Transição 2: Em Diagnóstico -> Em Execução
        var alterado2 = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEmExecucao(ordemId, CancellationToken.None));
        Assert.Equal(4, alterado2.Value!.Status); // EmExecucao = 4

        // Transição 3: Em Execução -> Finalizada
        var alterado3 = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaFinalizada(ordemId, CancellationToken.None));
        Assert.Equal(5, alterado3.Value!.Status); // Finalizada = 5

        // Transição 4: Finalizada -> Entregue
        var alterado4 = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.AlterarParaEntregue(ordemId, CancellationToken.None));
        Assert.Equal(6, alterado4.Value!.Status); // Entregue = 6

        // Verificar persistência
        var getFinal = Assert.IsType<Ok<OrdemServicoViewModel>>(
            await endpoints.GetById(ordemId, CancellationToken.None));
        Assert.Equal(6, getFinal.Value!.Status); // Entregue = 6
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var ordemIdInvalida = Guid.NewGuid();

        var result = Assert.IsType<NotFound<Dictionary<string, string>>>(
            await endpoints.AlterarParaEmDiagnostico(ordemIdInvalida, CancellationToken.None));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task AlterarParaEmExecucao_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var ordemIdInvalida = Guid.NewGuid();

        var result = Assert.IsType<NotFound<Dictionary<string, string>>>(
            await endpoints.AlterarParaEmExecucao(ordemIdInvalida, CancellationToken.None));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task AlterarParaFinalizada_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var ordemIdInvalida = Guid.NewGuid();

        var result = Assert.IsType<NotFound<Dictionary<string, string>>>(
            await endpoints.AlterarParaFinalizada(ordemIdInvalida, CancellationToken.None));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task AlterarParaEntregue_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var ordemIdInvalida = Guid.NewGuid();

        var result = Assert.IsType<NotFound<Dictionary<string, string>>>(
            await endpoints.AlterarParaEntregue(ordemIdInvalida, CancellationToken.None));

        Assert.NotNull(result);
    }
}
