using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Adapters.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Adapters.Tests.Features.OrdensServico;

public sealed class OrdensServicoAdapterTests
{
    private readonly OrdensServicoAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoOrdemServicoViewModelSucesso()
    {
        var ordem = new OrdemServicoViewModel { Id = Guid.NewGuid() };
        var result = new OrdensServicoResult<OrdemServicoViewModel, Exception>(ordem);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(adaptado);
        Assert.Equal(ordem, createdAtRoute.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoAberturaOrdemServicoViewModelSucesso()
    {
        var abertura = new AberturaOrdemServicoViewModel { OrdemServicoId = Guid.NewGuid() };
        var result = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(abertura);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<AberturaOrdemServicoViewModel>>(adaptado);
        Assert.Equal(abertura, createdAtRoute.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("erro");
        var result = new OrdensServicoResult<OrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode.StatusCode);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("nao encontrado");
        var result = new OrdensServicoResult<OrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, statusCode.StatusCode);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoListaOrdemServicoSucesso()
    {
        IReadOnlyCollection<OrdemServicoViewModel> ordens = [new OrdemServicoViewModel { Id = Guid.NewGuid() }];
        var result = new OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception>(ordens);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<OrdemServicoViewModel>>>(adaptado);
        Assert.Equal(ordens, ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoAcompanhamentoSucesso()
    {
        var acompanhamento = new AcompanhamentoOrdemServicoViewModel { Id = Guid.NewGuid(), Status = 1 };
        var result = new OrdensServicoResult<AcompanhamentoOrdemServicoViewModel, Exception>(acompanhamento);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<AcompanhamentoOrdemServicoViewModel>>(adaptado);
        Assert.Equal(acompanhamento, ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoListaAcompanhamentoSucesso()
    {
        IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel> acompanhamentos = [new AcompanhamentoOrdemServicoViewModel { Id = Guid.NewGuid(), Status = 1 }];
        var result = new OrdensServicoResult<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>, Exception>(acompanhamentos);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>>(adaptado);
        Assert.Equal(acompanhamentos, ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarOk_QuandoDeleteSucesso()
    {
        var result = new OrdensServicoResult<bool, Exception>(true);

        var adaptado = _adapter.Adapt(result);

        var ok = Assert.IsType<Ok<bool>>(adaptado);
        Assert.True(ok.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarNoContent_QuandoEmpty()
    {
        var adaptado = _adapter.Empty();

        Assert.IsType<NoContent>(adaptado);
    }
}
