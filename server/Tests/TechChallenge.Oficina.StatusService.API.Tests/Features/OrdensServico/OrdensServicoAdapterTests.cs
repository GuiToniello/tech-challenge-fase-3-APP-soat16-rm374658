using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.StatusService.API.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using Xunit;

namespace TechChallenge.Oficina.StatusService.API.Tests.Features.OrdensServico;

public sealed class OrdensServicoAdapterTests
{
    private readonly OrdensServicoAdapter _adapter = new();

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
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("nao encontrado");
        var result = new OrdensServicoResult<AcompanhamentoOrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, statusCode.StatusCode);
    }
}
