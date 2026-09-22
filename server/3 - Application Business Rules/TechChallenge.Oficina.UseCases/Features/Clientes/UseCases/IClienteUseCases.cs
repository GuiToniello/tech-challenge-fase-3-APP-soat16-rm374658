using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;

namespace TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;

public interface IClienteUseCases
{
    Task<ClienteViewModel> CriarAsync(CriarClienteCommand command, CancellationToken cancellationToken = default);
    Task<ClienteViewModel> AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default);
    Task<ClienteViewModel> ObterPorIdAsync(ObterClientePorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClienteViewModel>> ListarAsync(ListarClientesQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default);
}
