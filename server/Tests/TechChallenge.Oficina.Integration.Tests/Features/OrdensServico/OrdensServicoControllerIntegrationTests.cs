using Microsoft.AspNetCore.Http.HttpResults;
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

namespace TechChallenge.Oficina.Integration.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public OrdensServicoControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var cpf = _clienteCpfIndex++ == 0 ? "529.982.247-25" : "123.456.789-09";
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente OS", Identificacao = cpf, Email = $"os{cpf[..3]}@email.com" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private int _clienteCpfIndex;

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(
            new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901", ClienteId = clienteId },
            CancellationToken.None));
        return result.Value.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand { Nome = "Vela de Ignicao", Fabricante = "NGK", QuantidadeDisponivel = 20, ValorUnitario = 15m },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Vela",
                Descricao = "Substituicao das velas de ignicao",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 4 }]
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
    public async Task Post_OrdemServicoValida_DeveRetornar201ComViewModel()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(command, CancellationToken.None));
        var viewModel = Assert.IsType<OrdemServicoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal(clienteId, viewModel.ClienteId);
        Assert.Equal(veiculoId, viewModel.VeiculoId);
    }

    [Fact]
    public async Task PostCompleta_DadosValidos_DeveRetornar201ComIdentificacaoDaOs()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new AbrirOrdemServicoCompletaCommand
        {
            Cliente = new ClienteAberturaOrdemServicoCommand
            {
                NomeCompleto = "Cliente Abertura Completa",
                Identificacao = "529.982.247-25",
                Email = "abertura@oficina.com"
            },
            Veiculo = new VeiculoAberturaOrdemServicoCommand
            {
                Placa = "AAA1A11",
                Marca = "Chevrolet",
                Modelo = "Onix",
                Ano = 2023,
                Renavam = "12345678912"
            },
            Servicos =
            [
                new ServicoAberturaOrdemServicoCommand
                {
                    Nome = "Troca de Oleo",
                    Descricao = "Troca de oleo e filtro",
                    ItensServico =
                    [
                        new ItemServicoAberturaOrdemServicoCommand
                        {
                            Insumo = new InsumoAberturaOrdemServicoCommand
                            {
                                Nome = "Oleo 5W30",
                                Fabricante = "Mobil",
                                QuantidadeDisponivel = 20,
                                ValorUnitario = 35m
                            },
                            Quantidade = 4
                        }
                    ]
                }
            ]
        };

        var created = Assert.IsType<CreatedAtRoute<AberturaOrdemServicoViewModel>>(await endpoints.PostCompleta(command, CancellationToken.None));
        Assert.NotEqual(Guid.Empty, created.Value!.OrdemServicoId);

        var ordemCriada = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.GetById(created.Value.OrdemServicoId, CancellationToken.None));
        Assert.Equal(created.Value.OrdemServicoId, ordemCriada.Value.Id);
        Assert.NotEqual(Guid.Empty, ordemCriada.Value.ClienteId);
        Assert.NotEqual(Guid.Empty, ordemCriada.Value.VeiculoId);
        Assert.NotEmpty(ordemCriada.Value.Servicos);
    }

    [Fact]
    public async Task Post_ClienteInexistente_DeveRetornar404()
    {
        var (_, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = veiculoId, ServicoIds = [servicoId] };

        Assert.IsType<NotFound<Dictionary<string, string?>>>(await endpoints.Post(command, CancellationToken.None));
    }

    [Fact]
    public async Task Post_SemServicos_DeveRetornar400()
    {
        var (clienteId, veiculoId, _) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [] };

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(await endpoints.Post(command, CancellationToken.None));
    }

    [Fact]
    public async Task Post_VeiculoNaoPertenceAoCliente_DeveRetornar400()
    {
        var clientEndpoints = _fixture.CriarClientesEndpoints();
        var cliente1Result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Dono", Identificacao = "529.982.247-25", Email = "dono@email.com" },
            CancellationToken.None));
        var clienteDono = cliente1Result.Value!.Id;

        var cliente2Result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Ordem", Identificacao = "123.456.789-09", Email = "ordem@email.com" },
            CancellationToken.None));
        var clienteOrdem = cliente2Result.Value!.Id;

        var veiculoEndpoints = _fixture.CriarVeiculosEndpoints();
        var veiculoResult = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await veiculoEndpoints.Post(
            new CriarVeiculoCommand { Placa = "QQQ1Q11", Marca = "Fiat", Modelo = "Uno", Ano = 2020, Renavam = "00011122233", ClienteId = clienteDono },
            CancellationToken.None));
        var veiculoDoCliente1 = veiculoResult.Value.Id;

        var insumoEndpoints = _fixture.CriarInsumosEndpoints();
        var insumoResult = Assert.IsType<CreatedAtRoute<InsumoViewModel>>(await insumoEndpoints.Post(
            new CriarInsumoCommand { Nome = "Pastilha de Freio", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 40m },
            CancellationToken.None));
        var insumoId = insumoResult.Value!.Id;

        var servicoEndpoints = _fixture.CriarServicosEndpoints();
        var servicoResult = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await servicoEndpoints.Post(
            new CriarServicoCommand { Nome = "Troca Freio", Descricao = "Troca pastilha de freio", ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 2 }] },
            CancellationToken.None));
        var servicoId = servicoResult.Value.Id;

        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteOrdem, VeiculoId = veiculoDoCliente1, ServicoIds = [servicoId] };

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(await endpoints.Post(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetById_OrdemExistente_DeveRetornar200ComViewModel()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.GetById(id, CancellationToken.None));
        Assert.Equal(id, ok.Value.Id);
    }

    [Fact]
    public async Task GetById_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        Assert.IsType<NotFound<Dictionary<string, string?>>>(await endpoints.GetById(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Get_ComOrdensServico_DeveRetornar200ComLista()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        var resultado = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoViewModel>>>(await endpoints.Get(CancellationToken.None));
        var lista = Assert.IsAssignableFrom<IEnumerable<OrdemServicoViewModel>>(resultado.Value);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task GetAcompanhamento_OrdemExistente_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        Assert.IsType<Ok<AcompanhamentoOrdemServicoViewModel>>(await endpoints.GetAcompanhamento(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetByCliente_ClienteExistente_DeveRetornar200ComLista()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>>(await endpoints.GetByCliente(clienteId, CancellationToken.None));
        Assert.NotEmpty(ok.Value);
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_OrdemRecebida_DeveRetornar200ComStatusAtualizado()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None));
        Assert.Equal(2, ok.Value.Status);
    }

    [Fact]
    public async Task GerarOrcamento_OrdemEmDiagnostico_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);
        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.GerarOrcamento(id, CancellationToken.None));
        Assert.NotNull(ok.Value.Orcamento);
    }

    [Fact]
    public async Task FluxoCompleto_DevePassarPorTodosOsStatus()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);
        await endpoints.GerarOrcamento(id, CancellationToken.None);
        await endpoints.AprovarOrcamento(id, CancellationToken.None);
        await endpoints.AlterarParaEmExecucao(id, CancellationToken.None);
        await endpoints.AlterarParaFinalizada(id, CancellationToken.None);
        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.AlterarParaEntregue(id, CancellationToken.None));
        Assert.Equal(6, ok.Value.Status);
    }

    [Fact]
    public async Task RecusarOrcamento_OrdemAguardandoAprovacao_DeveRetornar200ComStatusFinalizada()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);
        await endpoints.GerarOrcamento(id, CancellationToken.None);
        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.RecusarOrcamento(id, CancellationToken.None));

        Assert.Equal(5, ok.Value.Status);
    }

    [Fact]
    public async Task RecusarOrcamento_OrdemSemOrcamento_DeveRetornar400()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(await endpoints.RecusarOrcamento(created.Value.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_OrdemExistente_DeveRetornar204()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;

        Assert.IsType<NoContent>(await endpoints.Delete(id, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        Assert.IsType<NotFound<Dictionary<string, string?>>>(await endpoints.Delete(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Put_OrdemExistente_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None));
        var id = created.Value.Id;
        var command = new AtualizarOrdemServicoCommand { Id = id, ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        Assert.IsType<Ok<OrdemServicoViewModel>>(await endpoints.Put(command, CancellationToken.None));
    }

    [Fact]
    public async Task Put_OrdemInexistente_DeveRetornar404()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new AtualizarOrdemServicoCommand { Id = Guid.NewGuid(), ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        Assert.IsType<NotFound<Dictionary<string, string?>>>(await endpoints.Put(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrdenadas_ComMultiplasOrdensComDiferentesStatus_DeveRetornarOrdenadasPorStatusEData()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

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

        // Chamar endpoint GetOrdenadas
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        var ordensOrdenadas = result.Value.ToList();

        // Verificar que as ordens retornadas estão ordenadas corretamente:
        // Prioridade: EmExecucao > AguardandoAprovacao > EmDiagnostico > Recebida
        Assert.NotEmpty(ordensOrdenadas);

        // Primeira ordem deve ser a que está em execução (ordem4)
        Assert.Equal(ordem4.Id, ordensOrdenadas[0].Id);
        Assert.Equal((int)TechChallenge.Oficina.Entities.Features.OrdensServico.Enums.StatusOrdemServico.EmExecucao, ordensOrdenadas[0].Status);
        // Verificar que DataAlteracao está preenchida
        Assert.All(ordensOrdenadas, o => Assert.NotEqual(default, o.DataAlteracao));

        // Verificar que as outras ordens seguem a ordem de prioridade e data
        var emDiagnostico = ordensOrdenadas.Where(o => o.Status == (int)TechChallenge.Oficina.Entities.Features.OrdensServico.Enums.StatusOrdemServico.EmDiagnostico).ToList();
        Assert.NotEmpty(emDiagnostico);
    }

    [Fact]
    public async Task GetOrdenadas_ComOrdensFinalizadasEEntregues_DeveExcluirDaListagem()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

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

        // Chamar endpoint GetOrdenadas
        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        var ordensOrdenadas = result.Value.ToList();

        // Deve conter apenas a ordem recebida, não as finalizadas ou entregues
        var ordemRecebidaRetornada = ordensOrdenadas.FirstOrDefault(o => o.Id == ordemRecebida.Id);
        Assert.NotNull(ordemRecebidaRetornada);

        var ordemFinalizadaRetornada = ordensOrdenadas.FirstOrDefault(o => o.Id == ordemFinalizada.Id);
        Assert.Null(ordemFinalizadaRetornada);

        var ordemEntregueRetornada = ordensOrdenadas.FirstOrDefault(o => o.Id == ordemEntregue.Id);
        Assert.Null(ordemEntregueRetornada);
    }

    [Fact]
    public async Task GetOrdenadas_SemOrdensAtivas_DeveRetornarListaVazia()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        var result = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>>>(
            await endpoints.GetOrdenadas(CancellationToken.None));

        Assert.Empty(result.Value);
    }
}
