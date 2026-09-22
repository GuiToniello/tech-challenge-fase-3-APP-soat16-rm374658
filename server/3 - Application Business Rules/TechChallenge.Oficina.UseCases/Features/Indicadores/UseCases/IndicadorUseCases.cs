using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Queries;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Entities.Features.Indicadores;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.Indicadores.Services;

public sealed class IndicadorUseCases : IIndicadorUseCases
{
    private readonly IIndicadorGateway _indicadorGateway;
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public IndicadorUseCases(IIndicadorGateway indicadorGateway, IOrdemServicoGateway ordemServicoGateway)
    {
        _indicadorGateway = indicadorGateway;
        _ordemServicoGateway = ordemServicoGateway;
    }

    public async Task<IndicadorViewModel> ObterAsync(ObterIndicadoresQuery query, CancellationToken cancellationToken = default)
    {
        var indicador = await _indicadorGateway.ObterAsync(cancellationToken);

        if (indicador is null)
        {
            return new IndicadorViewModel();
        }

        return new IndicadorViewModel
        {
            TempoMedioExecucao = indicador.TempoMedioExecucao,
            TempoMedioEntrega = indicador.TempoMedioEntrega
        };
    }

    public async Task AtualizarAsync(CancellationToken cancellationToken = default)
    {
        var ordensEntregues = await _ordemServicoGateway.ListarPorStatusAsync(StatusOrdemServico.Entregue, cancellationToken);
        var temposExecucao = ordensEntregues.Select(ordemServico => ordemServico.ObterTempoExecucao()).ToArray();
        var temposEntrega = ordensEntregues.Select(ordemServico => ordemServico.ObterTempoEntrega()).ToArray();

        var indicador = await _indicadorGateway.ObterAsync(cancellationToken) ?? Indicador.Criar(TimeSpan.Zero, TimeSpan.Zero);
        indicador.Atualizar(CalcularMedia(temposExecucao), CalcularMedia(temposEntrega));
        await _indicadorGateway.SalvarAsync(indicador, cancellationToken);
    }

    private static TimeSpan CalcularMedia(TimeSpan[] intervalos)
    {
        if (intervalos.Length == 0)
        {
            return TimeSpan.Zero;
        }

        var mediaTicks = intervalos.Average(intervalo => intervalo.Ticks);
        return TimeSpan.FromTicks(Convert.ToInt64(mediaTicks));
    }
}
