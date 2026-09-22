using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Servicos
{
    public class ServicoController : IServicoController
    {
        private readonly IServicoUseCases _servicoUseCases;
        private readonly IServicoAdapter _servicoAdapter;

        public ServicoController(IServicoUseCases servicoUsecases, IServicoAdapter servicoAdapter)
        {
            _servicoUseCases = servicoUsecases;
            _servicoAdapter = servicoAdapter;
        }

        public async Task<object> Post(CriarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoUseCases.CriarAsync(command, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterServicoPorIdQuery { Id = id };
                var servico = await _servicoUseCases.ObterPorIdAsync(query, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var servicos = await _servicoUseCases.ListarAsync(new ListarServicosQuery(), cancellationToken);
            var result = ServicoResult.From(servicos);

            return _servicoAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoUseCases.AtualizarAsync(command, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirServicoCommand { Id = id };
                await _servicoUseCases.ExcluirAsync(command, cancellationToken);

                return _servicoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<bool>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }
    }
}
