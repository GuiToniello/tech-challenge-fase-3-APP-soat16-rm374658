using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Veiculos;

public sealed class VeiculosControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public VeiculosControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteAsync()
    {
        var clientEndpoints = _fixture.CriarClientesEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Teste", Identificacao = "529.982.247-25" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private static CriarVeiculoCommand VeiculoValido(Guid clienteId) => new()
    {
        Placa = "ABC1D23",
        Marca = "Toyota",
        Modelo = "Corolla",
        Ano = 2023,
        Renavam = "12345678901",
        ClienteId = clienteId
    };

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_VeiculoValido_DeveRetornar201ComViewModel()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var command = VeiculoValido(clienteId);

        // Act
        var resultado = await endpoints.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(resultado);
        Assert.NotEqual(Guid.Empty, created.Value!.Id);
        Assert.Equal("ABC1D23", created.Value.Placa);
        Assert.Equal(clienteId, created.Value.ClienteId);
    }

    [Fact]
    public async Task Post_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var command = VeiculoValido(Guid.NewGuid());

        // Act
        var resultado = await endpoints.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task Post_PlacaInvalida_DeveRetornar400()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var command = new CriarVeiculoCommand
        {
            Placa = "INVALIDA",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await endpoints.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado);
    }

    [Fact]
    public async Task Post_PlacaDuplicada_DeveRetornar400()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Act
        var resultado = await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_VeiculoExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None));
        var id = created.Value!.Id;

        // Act
        var resultado = await endpoints.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<Ok<VeiculoViewModel>>(resultado);
        Assert.Equal(id, ok.Value!.Id);
    }

    [Fact]
    public async Task GetById_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var endpoints = _fixture.CriarVeiculosEndpoints();

        // Act
        var resultado = await endpoints.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista por clienteId
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_FiltrandoPorClienteId_DeveRetornar200SomenteVeiculosDoCliente()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Act
        var resultado = await endpoints.Get(clienteId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<Ok<IReadOnlyCollection<VeiculoViewModel>>>(resultado);
        Assert.All(ok.Value!, v => Assert.Equal(clienteId, v.ClienteId));
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_VeiculoExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None));
        var id = created.Value!.Id;
        var command = new AtualizarVeiculoCommand
        {
            Id = id,
            Placa = "XYZ9W88",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2024,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await endpoints.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<Ok<VeiculoViewModel>>(resultado);
        Assert.Equal("XYZ9W88", ok.Value!.Placa);
        Assert.Equal("Honda", ok.Value.Marca);
    }

    [Fact]
    public async Task Put_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var command = new AtualizarVeiculoCommand
        {
            Id = Guid.NewGuid(),
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await endpoints.Put(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }

    // ------------------------------------------------------------------ //
    // DELETE
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Delete_VeiculoExistente_DeveRetornar204()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var created = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(VeiculoValido(clienteId), CancellationToken.None));
        var id = created.Value!.Id;

        // Act
        var resultado = await endpoints.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(resultado);
    }

    [Fact]
    public async Task Delete_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var endpoints = _fixture.CriarVeiculosEndpoints();

        // Act
        var resultado = await endpoints.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado);
    }
}
