using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public class ClienteController : IClienteController
    {
        private readonly IClienteUseCases _clienteUsecases;
        private readonly IClienteAdapter _clientAdapter;

        public ClienteController(IClienteUseCases clienteUsecases, IClienteAdapter clientAdapter)
        {
            _clienteUsecases = clienteUsecases;
            _clientAdapter = clientAdapter;
        }

        public async Task<object> Post(CriarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteUsecases.CriarAsync(command, cancellationToken);
                var result = ClienteResult.From(cliente);

                return _clientAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);

                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterClientePorIdQuery { Id = id };
                var cliente = await _clienteUsecases.ObterPorIdAsync(query, cancellationToken);
                var result = ClienteResult.From(cliente);

                return _clientAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var clientes = await _clienteUsecases.ListarAsync(new ListarClientesQuery(), cancellationToken);
            var result = ClienteResult.From(clientes);

            return _clientAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteUsecases.AtualizarAsync(command, cancellationToken);
                var result = ClienteResult.From(cliente);
                return _clientAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirClienteCommand { Id = id };
                await _clienteUsecases.ExcluirAsync(command, cancellationToken);

                return _clientAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<bool>(exception);
                return _clientAdapter.Adapt(result);
            }
        }
    }
}
