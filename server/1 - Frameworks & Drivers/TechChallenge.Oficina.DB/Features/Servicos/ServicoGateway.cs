using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;

namespace TechChallenge.Oficina.DB.Data.Features.Servicos;

public sealed class ServicoGateway : IServicoGateway
{
    private readonly OficinaDbContext _context;

    public ServicoGateway(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        await _context.Servicos.AddAsync(servico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        _context.Servicos.Update(servico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Servicos
            .Include(servico => servico.ItensServico)
            .ThenInclude(itemServico => itemServico.Insumo)
            .FirstOrDefaultAsync(servico => servico.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Servico>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Servicos
            .Include(servico => servico.ItensServico)
            .ThenInclude(itemServico => itemServico.Insumo)
            .OrderBy(servico => servico.Nome)
            .ToArrayAsync(cancellationToken);
    }

    public async Task RemoverAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        _context.Servicos.Remove(servico);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
