using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Monolith.API.Features.Clientes;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Clientes;

public sealed class ClientEndpointsIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public ClientEndpointsIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Post_ClienteValido_DeveRetornar201ComViewModel()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var command = new CriarClienteCommand
        {
            NomeCompleto = "João da Silva",
            Identificacao = "529.982.247-25",
            Email = "joao@email.com"
        };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(resultado);
        Assert.NotEqual(Guid.Empty, created.Value!.Id);
        Assert.Equal("João da Silva", created.Value.NomeCompleto);
        Assert.Equal("joao@email.com", created.Value.Email);
    }

    [Fact]
    public async Task Post_CpfInvalido_DeveRetornar400()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var command = new CriarClienteCommand
        {
            NomeCompleto = "Maria Souza",
            Identificacao = "111.111.111-11"
        };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var command = new CriarClienteCommand
        {
            NomeCompleto = " ",
            Identificacao = "529.982.247-25"
        };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task Post_IdentificacaoDuplicada_DeveRetornar400()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var cpf = "529.982.247-25";
        await endpoints.Post(new CriarClienteCommand { NomeCompleto = "Cliente Original", Identificacao = cpf }, CancellationToken.None);

        var resultado = await endpoints.Post(new CriarClienteCommand { NomeCompleto = "Outro Cliente", Identificacao = cpf }, CancellationToken.None);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetById_ClienteExistente_DeveRetornar200ComViewModel()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Ana Lima", Identificacao = "529.982.247-25" },
            CancellationToken.None));
        var id = created.Value.Id;

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<ClienteViewModel>>(resultado);
        Assert.NotNull(ok.Value);
        Assert.Equal(id, ok.Value.Id);
    }

    [Fact]
    public async Task GetById_ClienteInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarClientesEndpoints();

        var resultado = await endpoints.GetById(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Get_SemClientes_DeveRetornar200ComListaVazia()
    {
        var endpoints = _fixture.CriarClientesEndpoints();

        var resultado = await endpoints.Get(CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<ClienteViewModel>>>(resultado);
        Assert.NotNull(ok.Value);
        Assert.Empty(ok.Value);
    }

    [Fact]
    public async Task Get_ComClientes_DeveRetornar200ComLista()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        await endpoints.Post(new CriarClienteCommand { NomeCompleto = "Pedro Costa", Identificacao = "529.982.247-25" }, CancellationToken.None);

        var resultado = await endpoints.Get(CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<ClienteViewModel>>>(resultado);
        Assert.NotNull(ok.Value);
        Assert.NotEmpty(ok.Value);
    }

    [Fact]
    public async Task Put_ClienteExistente_DeveRetornar200ComDadosAtualizados()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Fernanda Rocha", Identificacao = "529.982.247-25" },
            CancellationToken.None));
        var id = created.Value.Id;
        var command = new AtualizarClienteCommand
        {
            Id = id,
            NomeCompleto = "Fernanda Rocha Atualizada",
            Identificacao = "529.982.247-25"
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var ok = Assert.IsType<Ok<ClienteViewModel>>(resultado);
        Assert.NotNull(ok.Value);
        Assert.Equal("Fernanda Rocha Atualizada", ok.Value.NomeCompleto);
    }

    [Fact]
    public async Task Put_ClienteInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var command = new AtualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Nome Qualquer",
            Identificacao = "529.982.247-25"
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Delete_ClienteExistente_DeveRetornar204()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var created = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Lucas Martins", Identificacao = "529.982.247-25" },
            CancellationToken.None));
        var id = created.Value.Id;

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado);
    }

    [Fact]
    public async Task Delete_ClienteInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarClientesEndpoints();

        var resultado = await endpoints.Delete(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }
}
