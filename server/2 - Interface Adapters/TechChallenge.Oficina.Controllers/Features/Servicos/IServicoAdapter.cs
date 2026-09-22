using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Servicos
{
    public interface IServicoAdapter
    {
        object Adapt(ServicoResult<ServicoViewModel, Exception> result, bool created = false);

        object Adapt(ServicoResult<IReadOnlyCollection<ServicoViewModel>, Exception> result);

        object Adapt(ServicoResult<bool, Exception> result);

        object Empty();
    }
}
