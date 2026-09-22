using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Insumos
{
    public class InsumoController : IInsumoController
    {
        private readonly IInsumoUseCases _insumoUsecases;
        private readonly IInsumoAdapter _insumoAdapter;

        public InsumoController(IInsumoUseCases insumoUsecases, IInsumoAdapter insumoAdapter)
        {
            _insumoUsecases = insumoUsecases;
            _insumoAdapter = insumoAdapter;
        }

        public async Task<object> Post(CriarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoUsecases.CriarAsync(command, cancellationToken);
                var result = InsumoResult.From(insumo);

                return _insumoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);

                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterInsumoPorIdQuery { Id = id };
                var insumo = await _insumoUsecases.ObterPorIdAsync(query, cancellationToken);
                var result = InsumoResult.From(insumo);

                return _insumoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var insumos = await _insumoUsecases.ListarAsync(new ListarInsumosQuery(), cancellationToken);
            var result = InsumoResult.From(insumos);

            return _insumoAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoUsecases.AtualizarAsync(command, cancellationToken);
                var result = InsumoResult.From(insumo);
                return _insumoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirInsumoCommand { Id = id };
                await _insumoUsecases.ExcluirAsync(command, cancellationToken);

                return _insumoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<bool>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }
    }
}
