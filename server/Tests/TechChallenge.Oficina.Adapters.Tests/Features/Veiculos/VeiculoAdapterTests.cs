using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Adapters.Features.Veiculos;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Adapters.Tests.Features.Veiculos;

public sealed class VeiculoAdapterTests
{
    private readonly VeiculoAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoVeiculoViewModelSucesso()
    {
        var veiculo = new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" };
        var result = new VeiculoResult<VeiculoViewModel, Exception>(veiculo);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(adaptado);
        Assert.Equal(veiculo, createdAtRoute.Value);
        Assert.Equal("GetVeiculoById", createdAtRoute.RouteName);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("Placa inválida");
        var result = new VeiculoResult<VeiculoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Placa inválida", badRequest.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Veículo não encontrado");
        var result = new VeiculoResult<VeiculoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Veículo não encontrado", notFound.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoExceptionGenerica()
    {
        var exception = new InvalidOperationException("Erro inesperado");
        var result = new VeiculoResult<VeiculoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoDeleteSucesso()
    {
        var result = new VeiculoResult<bool, Exception>(true);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<bool>>(adaptado);
        Assert.True(ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoDeleteComKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Veículo não encontrado");
        var result = new VeiculoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Veículo não encontrado", notFound.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDeleteComDomainException()
    {
        var exception = new DomainException("Erro de validação");
        var result = new VeiculoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Erro de validação", badRequest.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoDeleteComExceptionGenerica()
    {
        var exception = new TimeoutException("Timeout na operação");
        var result = new VeiculoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Empty_DeveRetornarNoContent()
    {
        var adaptado = _adapter.Empty();

        Assert.IsType<NoContent>(adaptado);
    }
}
