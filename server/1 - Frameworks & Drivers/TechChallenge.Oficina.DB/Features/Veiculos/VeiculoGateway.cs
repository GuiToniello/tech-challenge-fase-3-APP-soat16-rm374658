using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.Veiculos.VOs;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;

namespace TechChallenge.Oficina.DB.Data.Features.Veiculos;

public sealed class VeiculoGateway : IVeiculoGateway
{
    private readonly OficinaDbContext _context;

    public VeiculoGateway(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        await _context.Veiculos.AddAsync(veiculo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        _context.Veiculos.Update(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Veiculos
            .FirstOrDefaultAsync(veiculo => veiculo.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Veiculo>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Veiculos
            .OrderBy(veiculo => veiculo.Marca)
            .ThenBy(veiculo => veiculo.Modelo)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Veiculos
            .Where(veiculo => veiculo.ClienteId == clienteId)
            .OrderBy(veiculo => veiculo.Marca)
            .ThenBy(veiculo => veiculo.Modelo)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> ExisteComPlacaAsync(string placa, Guid? ignorarVeiculoId = null, CancellationToken cancellationToken = default)
    {
        var placaMercosul = PlacaMercosul.Criar(placa);
        return await _context.Veiculos.AnyAsync(
            veiculo => veiculo.Placa.Equals(placaMercosul)
                && (!ignorarVeiculoId.HasValue || veiculo.Id != ignorarVeiculoId.Value),
            cancellationToken);
    }

    public async Task RemoverAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        _context.Veiculos.Remove(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
