using System;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Indicadores
{
    public interface IIndicadoresAdapter
    {
        object Adapt(IndicadoresResult<IndicadorViewModel, Exception> result);

        object Adapt(IndicadoresResult<IReadOnlyCollection<IndicadorViewModel>, Exception> result);

        object Adapt(IndicadoresResult<bool, Exception> result);

        object Empty();
    }
}
