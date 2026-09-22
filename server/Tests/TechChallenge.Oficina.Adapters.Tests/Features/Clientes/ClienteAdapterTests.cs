using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Adapters.Features.Clientes;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Adapters.Tests.Features.Clientes;

public sealed class ClienteAdapterTests
{
    private readonly ClienteAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoClienteViewModelSucesso()
    {
        var cliente = new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "João Silva" };
        var result = new ClienteResult<ClienteViewModel, Exception>(cliente);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(adaptado);
        Assert.Equal(cliente, createdAtRoute.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("Nome inválido");
        var result = new ClienteResult<ClienteViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Nome inválido", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Cliente não encontrado");
        var result = new ClienteResult<ClienteViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Cliente não encontrado", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoExceptionGenerica()
    {
        var exception = new InvalidOperationException("Erro inesperado");
        var result = new ClienteResult<ClienteViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoDeleteSucesso()
    {
        var result = new ClienteResult<bool, Exception>(true);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<bool>>(adaptado);
        Assert.True(ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoDeleteComKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Cliente não encontrado");
        var result = new ClienteResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(adaptado);
        Assert.Equal("Cliente não encontrado", valueResult.Value?.GetType().GetProperty("Message")?.GetValue(valueResult.Value));
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDeleteComDomainException()
    {
        var exception = new DomainException("Erro de validação");
        var result = new ClienteResult<bool, Exception>(exception);

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
        var result = new ClienteResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }
}
