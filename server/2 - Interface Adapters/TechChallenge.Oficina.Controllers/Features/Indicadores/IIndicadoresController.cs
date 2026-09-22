namespace TechChallenge.Oficina.Controllers.Features.Indicadores
{
    public interface IIndicadoresController
    {
        Task<object> Get(CancellationToken cancellationToken);
    }
}
