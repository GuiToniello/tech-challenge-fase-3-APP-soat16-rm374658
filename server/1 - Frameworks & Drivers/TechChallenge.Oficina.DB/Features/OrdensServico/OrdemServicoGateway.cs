using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

namespace TechChallenge.Oficina.DB.Data.Features.OrdensServico;

public sealed class OrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OficinaDbContext _context;

    public OrdemServicoGateway(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        await _context.OrdensServico.AddAsync(ordemServico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        _context.OrdensServico.Update(ordemServico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .FirstOrDefaultAsync(ordemServico => ordemServico.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarOrdenadasAsync(CancellationToken cancellationToken = default)
    {
        var ordensServico = await CriarConsultaCompleta()
            .ToArrayAsync(cancellationToken);

        // Filtrar ordens que não estão finalizadas, entregues ou encerradas
        var ordensAtivasOuAguardando = ordensServico
            .Where(os => os.Status != StatusOrdemServico.Finalizada && 
                         os.Status != StatusOrdemServico.Entregue && 
                         os.Status != StatusOrdemServico.Encerrada)
            .ToArray();

        // Ordenar por prioridade de status e depois por data de criação (primeira entrada do histórico)
        var prioridadeStatus = new Dictionary<StatusOrdemServico, int>
        {
            { StatusOrdemServico.EmExecucao, 1 },
            { StatusOrdemServico.AguardandoAprovacao, 2 },
            { StatusOrdemServico.EmDiagnostico, 3 },
            { StatusOrdemServico.Recebida, 4 }
        };

        var ordensOrdenadas = ordensAtivasOuAguardando
            .OrderBy(os => prioridadeStatus.TryGetValue(os.Status, out var prioridade) ? prioridade : int.MaxValue)
            .ThenBy(os => os.HistoricoStatus.FirstOrDefault()?.DataAlteracao ?? DateTime.MaxValue)
            .ToArray();

        return ordensOrdenadas;
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .Where(ordemServico => ordemServico.ClienteId == clienteId)
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarPorStatusAsync(StatusOrdemServico status, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .Where(ordemServico => ordemServico.Status == status)
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task RemoverAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        _context.OrdensServico.Remove(ordemServico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<OrdemServico> CriarConsultaCompleta()
    {
        return _context.OrdensServico
            .Include(ordemServico => ordemServico.Servicos)
                .ThenInclude(servico => servico.ItensServico)
                .ThenInclude(itemServico => itemServico.Insumo)
            .Include(ordemServico => ordemServico.Orcamento)
                .ThenInclude(orcamento => orcamento!.Servicos)
            .Include(ordemServico => ordemServico.HistoricoStatus);
    }
}
