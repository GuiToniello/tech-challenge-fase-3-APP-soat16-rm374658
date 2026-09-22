using TechChallenge.Oficina.UseCases.Features.Indicadores.Queries;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

public interface IIndicadorUseCases : Entities.Features.Indicadores.IIndicadorService<ObterIndicadoresQuery, IndicadorViewModel>
{
}
