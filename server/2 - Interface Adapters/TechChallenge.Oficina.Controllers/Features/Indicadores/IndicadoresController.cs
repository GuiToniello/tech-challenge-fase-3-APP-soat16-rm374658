using TechChallenge.Oficina.UseCases.Features.Indicadores.Queries;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

namespace TechChallenge.Oficina.Controllers.Features.Indicadores
{
    public sealed class IndicadoresController : IIndicadoresController
    {
        private readonly IIndicadorUseCases _indicadorUsecases;
        private readonly IIndicadoresAdapter _indicadoresAdapter;

        public IndicadoresController(IIndicadorUseCases indicadorUsecases, IIndicadoresAdapter indicadoresAdapter)
        {
            _indicadorUsecases = indicadorUsecases;
            _indicadoresAdapter = indicadoresAdapter;
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var indicadores = await _indicadorUsecases.ObterAsync(new ObterIndicadoresQuery(), cancellationToken);
            var result =  IndicadoresResult.From(indicadores);

            return _indicadoresAdapter.Adapt(result);
        }
    }
}
