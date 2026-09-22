using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Veiculos
{
    public interface IVeiculoAdapter
    {
        object Adapt(VeiculoResult<VeiculoViewModel, Exception> result, bool created = false);

        object Adapt(VeiculoResult<IReadOnlyCollection<VeiculoViewModel>, Exception> result);

        object Adapt(VeiculoResult<bool, Exception> result);

        object Empty();
    }
}
