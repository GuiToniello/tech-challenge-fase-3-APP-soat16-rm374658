using System;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public interface IClienteAdapter
    {
        object Adapt(ClienteResult<ClienteViewModel, Exception> result, bool created = false);

        object Adapt(ClienteResult<IReadOnlyCollection<ClienteViewModel>, Exception> result);

        object Adapt(ClienteResult<bool, Exception> result);

        object Empty();
    }
}
