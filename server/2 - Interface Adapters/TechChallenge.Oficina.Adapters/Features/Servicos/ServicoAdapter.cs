using Microsoft.AspNetCore.Http;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Adapters.Features.Servicos
{
    public class ServicoAdapter : IServicoAdapter
    {
        public object Adapt(ServicoResult<ServicoViewModel, Exception> result, bool created = false)
        {
            if (result.Value != null)
                if (created)
                    return TypedResults.CreatedAtRoute(result.Value, "GetServicoById", new { id = result.Value.Id });
                else
                    return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception> result)
        {
            if (result.Value != null)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(ServicoResult<bool, Exception> result)
        {
            if (result.Value)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Empty()
        {
            return TypedResults.NoContent();
        }

        public object CriaErro(Exception ex)
        {
            if (ex is DomainException)
                return TypedResults.BadRequest(new Dictionary<string, string?> { ["message"] = ex.Message });

            if (ex is KeyNotFoundException)
                return TypedResults.NotFound(new Dictionary<string, string?> { ["message"] = ex.Message });

            return TypedResults.Problem(ex?.Message);
        }
    }
}
