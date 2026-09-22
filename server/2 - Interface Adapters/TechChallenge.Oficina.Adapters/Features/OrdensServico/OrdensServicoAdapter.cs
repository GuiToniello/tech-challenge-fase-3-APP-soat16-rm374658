using Microsoft.AspNetCore.Http;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Adapters.Features.OrdensServico
{
    public class OrdensServicoAdapter : IOrdensServicoAdapter
    {
        public object Adapt(OrdensServicoResult<OrdemServicoViewModel, Exception> result, bool created = false)
        {
            if (result.Value != null)
            {
                if (created)
                {
                    return TypedResults.CreatedAtRoute(result.Value, "GetOrdemServicoById", new { id = result.Value.Id });
                }

                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<AberturaOrdemServicoViewModel, Exception> result, bool created = false)
        {
            if (result.Value != null)
            {
                if (created)
                {
                    return TypedResults.CreatedAtRoute(result.Value, "GetOrdemServicoById", new { id = result.Value.OrdemServicoId });
                }

                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception> result)
        {
            if (result.Value != null)
            {
                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<IReadOnlyCollection<OrdemServicoOrdenadasViewModel>, Exception> result)
        {
            if (result.Value != null)
            {
                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<AcompanhamentoOrdemServicoViewModel, Exception> result)
        {
            if (result.Value != null)
            {
                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>, Exception> result)
        {
            if (result.Value != null)
            {
                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Adapt(OrdensServicoResult<bool, Exception> result)
        {
            if (result.Value)
            {
                return TypedResults.Ok(result.Value);
            }

            return CriaErro(result.Error!);
        }

        public object Empty()
        {
            return TypedResults.NoContent();
        }

        public object CriaErro(Exception ex)
        {
            if (ex is DomainException)
            {
                return TypedResults.BadRequest(new Dictionary<string, string?> { ["message"] = ex.Message });
            }

            if (ex is KeyNotFoundException)
            {
                return TypedResults.NotFound(new Dictionary<string, string?> { ["message"] = ex.Message });
            }

            return TypedResults.Problem(ex?.Message);
        }
    }
}
