using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Servicos;

public sealed class ServicosControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public ServicosControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarInsumoAsync(string nome = "Filtro de Ar")
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand { Nome = nome, Fabricante = "Mann", QuantidadeDisponivel = 50, ValorUnitario = 25m },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private static CriarServicoCommand ServicoSemItens(string nome = "Revisão Geral") => new()
    {
        Nome = nome,
        Descricao = "Revisão completa do veículo",
        ItensServico = []
    };

    [Fact]
    public async Task Post_ServicoValido_DeveRetornar201ComViewModel()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var command = ServicoSemItens();

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("Revisão Geral", viewModel.Nome);
    }

    [Fact]
    public async Task Post_ServicoComItens_DeveRetornar201ComItensAssociados()
    {
        var insumoId = await CriarInsumoAsync();
        var endpoints = _fixture.CriarServicosEndpoints();
        var command = new CriarServicoCommand
        {
            Nome = "Troca de Filtro",
            Descricao = "Substituição do filtro de ar",
            ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
        };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(created.Value);
        Assert.Single(viewModel.ItensServico);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var command = new CriarServicoCommand { Nome = " ", Descricao = "Descrição válida", ItensServico = [] };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task Post_InsumoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var command = new CriarServicoCommand
        {
            Nome = "Serviço X",
            Descricao = "Desc X",
            ItensServico = [new ItemServicoCommand { InsumoId = Guid.NewGuid(), Quantidade = 1 }]
        };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task GetById_ServicoExistente_DeveRetornar200ComViewModel()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(ServicoSemItens(), CancellationToken.None));
        var id = created.Value.Id;

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<ServicoViewModel>>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_ServicoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarServicosEndpoints();

        var resultado = await endpoints.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task Get_ComServicos_DeveRetornar200ComLista()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        await endpoints.Post(ServicoSemItens("Alinhamento"), CancellationToken.None);

        var resultado = await endpoints.Get(CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<ServicoViewModel>>>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<ServicoViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task Put_ServicoExistente_DeveRetornar200ComDadosAtualizados()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(ServicoSemItens(), CancellationToken.None));
        var id = created.Value.Id;
        var command = new AtualizarServicoCommand
        {
            Id = id,
            Nome = "Revisão Geral Atualizada",
            Descricao = "Revisão completa atualizada",
            ItensServico = []
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var ok = Assert.IsType<Ok<ServicoViewModel>>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(ok.Value);
        Assert.Equal("Revisão Geral Atualizada", viewModel.Nome);
    }

    [Fact]
    public async Task Put_ServicoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var command = new AtualizarServicoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Qualquer",
            Descricao = "Desc",
            ItensServico = []
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task Delete_ServicoExistente_DeveRetornar204()
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(await endpoints.Post(ServicoSemItens(), CancellationToken.None));
        var id = created.Value.Id;

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado);
    }

    [Fact]
    public async Task Delete_ServicoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarServicosEndpoints();

        var resultado = await endpoints.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }
}
