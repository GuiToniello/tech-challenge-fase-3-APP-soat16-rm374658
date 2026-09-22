using Microsoft.AspNetCore.Http;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Adapters.Features.Clientes
{
    public class ClienteAdapter : IClienteAdapter
    {
        public object Adapt(ClienteResult<ClienteViewModel, Exception> result, bool created = false)
        {
            if (result.Value != null)
                if (created)
                    return TypedResults.CreatedAtRoute(result.Value, "PostCliente", new { id = result.Value.Id });
                else
                    return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }


        public object Adapt(ClienteResult<bool, Exception> result)
        {
            if (result.Value)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception> result)
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
