using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

namespace TechChallenge.Oficina.DB.Data.Features.Insumos;

public sealed class InsumoGateway : IInsumoGateway
{
    private readonly OficinaDbContext _context;

    public InsumoGateway(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        await _context.Insumos.AddAsync(insumo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        _context.Insumos.Update(insumo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Insumo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Insumos
            .FirstOrDefaultAsync(insumo => insumo.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Insumo>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Insumos
            .OrderBy(insumo => insumo.Nome)
            .ThenBy(insumo => insumo.Fabricante)
            .ToArrayAsync(cancellationToken);
    }

    public async Task RemoverAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        _context.Insumos.Remove(insumo);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
