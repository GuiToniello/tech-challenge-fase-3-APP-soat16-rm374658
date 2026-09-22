using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Adapters.Features.Servicos;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Adapters.Tests.Features.Servicos;

public sealed class ServicoAdapterTests
{
    private readonly ServicoAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoServicoViewModelSucesso()
    {
        var servico = new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" };
        var result = new ServicoResult<ServicoViewModel, Exception>(servico);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(adaptado);
        Assert.Equal(servico, createdAtRoute.Value);
        Assert.Equal("GetServicoById", createdAtRoute.RouteName);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("Serviço inválido");
        var result = new ServicoResult<ServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Serviço inválido", badRequest.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Serviço não encontrado");
        var result = new ServicoResult<ServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Serviço não encontrado", notFound.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoExceptionGenerica()
    {
        var exception = new InvalidOperationException("Erro inesperado");
        var result = new ServicoResult<ServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoDeleteSucesso()
    {
        var result = new ServicoResult<bool, Exception>(true);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<bool>>(adaptado);
        Assert.True(ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoDeleteComKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Serviço não encontrado");
        var result = new ServicoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Serviço não encontrado", notFound.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDeleteComDomainException()
    {
        var exception = new DomainException("Erro de validação");
        var result = new ServicoResult<bool, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(adaptado);
        Assert.Equal("Erro de validação", badRequest.Value?["message"]);
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoDeleteComExceptionGenerica()
    {
        var exception = new TimeoutException("Timeout na operação");
        var result = new ServicoResult<bool, Exception>(exception);

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
