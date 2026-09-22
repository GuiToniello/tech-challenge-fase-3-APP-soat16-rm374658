using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Adapters.Features.Insumos;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Adapters.Tests.Features.Insumos;

public sealed class InsumoAdapterTests
{
    private readonly InsumoAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoInsumoViewModelSucesso()
    {
        var insumo = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Óleo" };
        var result = new InsumoResult<InsumoViewModel, Exception>(insumo);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<InsumoViewModel>>(adaptado);
        Assert.Equal(insumo, createdAtRoute.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoInsumoViewModelSemCreated()
    {
        var insumo = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Filtro" };
        var result = new InsumoResult<InsumoViewModel, Exception>(insumo);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<InsumoViewModel>>(adaptado);
        Assert.Equal(insumo, ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("Nome inválido");
        var result = new InsumoResult<InsumoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Nome inválido", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Insumo não encontrado");
        var result = new InsumoResult<InsumoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Insumo não encontrado", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoExceptionGenerica()
    {
        var exception = new InvalidOperationException("Erro inesperado");
        var result = new InsumoResult<InsumoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoDeleteSucesso()
    {
        var result = new InsumoResult<bool, Exception>(true);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<bool>>(adaptado);
        Assert.True(ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoDeleteComKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Insumo não encontrado");
        var result = new InsumoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Insumo não encontrado", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDeleteComDomainException()
    {
        var exception = new DomainException("Erro de validação");
        var result = new InsumoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Erro de validação", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoDeleteComExceptionGenerica()
    {
        var exception = new TimeoutException("Timeout na operação");
        var result = new InsumoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoColecaoComSucesso()
    {
        IReadOnlyCollection<InsumoViewModel> insumos = [new InsumoViewModel { Nome = "Óleo" }];
        var result = new InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception>(insumos);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<InsumoViewModel>>>(adaptado);
        Assert.Equal(insumos, ok.Value);
    }

    [Fact]
    public void Empty_DeveRetornarNoContent()
    {
        var adaptado = _adapter.Empty();

        Assert.IsType<NoContent>(adaptado);
    }
}
