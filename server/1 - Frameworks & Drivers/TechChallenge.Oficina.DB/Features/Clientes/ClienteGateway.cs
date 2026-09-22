using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;

namespace TechChallenge.Oficina.DB.Data.Features.Clientes;

public sealed class ClienteGateway : IClienteGateway
{
    private readonly OficinaDbContext _context;

    public ClienteGateway(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Cliente>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .OrderBy(cliente => cliente.NomeCompleto)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExisteComIdentificacaoAsync(string identificacaoNormalizada, Guid? ignorarClienteId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes.AnyAsync(
            cliente => cliente.Identificacao.Valor == identificacaoNormalizada
                && (!ignorarClienteId.HasValue || cliente.Id != ignorarClienteId.Value),
            cancellationToken);
    }

    public async Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
