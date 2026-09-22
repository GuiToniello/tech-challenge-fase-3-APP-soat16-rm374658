namespace TechChallenge.Oficina.Entities.Features.Indicadores;

public interface IIndicadorService<TObterQuery, TViewModel>
{
    Task<TViewModel> ObterAsync(TObterQuery query, CancellationToken cancellationToken = default);
    Task AtualizarAsync(CancellationToken cancellationToken = default);
}
