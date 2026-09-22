using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Insumos
{
    public interface IInsumoAdapter
    {
        object Adapt(InsumoResult<InsumoViewModel, Exception> result, bool created = false);

        object Adapt(InsumoResult<IReadOnlyCollection<InsumoViewModel>, Exception> result);

        object Adapt(InsumoResult<bool, Exception> result);

        object Empty();
    }
}
