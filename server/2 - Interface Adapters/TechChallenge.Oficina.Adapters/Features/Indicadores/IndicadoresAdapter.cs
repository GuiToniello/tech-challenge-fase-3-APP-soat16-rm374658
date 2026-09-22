using Microsoft.AspNetCore.Http;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Indicadores;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Adapters.Features.Indicadores
{
    public class IndicadoresAdapter : IIndicadoresAdapter
    {
        public object Adapt(IndicadoresResult<IndicadorViewModel, Exception> result)
        {
            if (result.Value != null)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(IndicadoresResult<bool, Exception> result)
        {
            if (result.Value)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(IndicadoresResult<IReadOnlyCollection<IndicadorViewModel>, Exception> result)
        {
            if (result.Value != null)
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
                return TypedResults.BadRequest(new { Message = ex.Message });

            if (ex is KeyNotFoundException)
                return TypedResults.NotFound(new { Message = ex.Message });

            return TypedResults.Problem(ex?.Message);
        }
    }
}
