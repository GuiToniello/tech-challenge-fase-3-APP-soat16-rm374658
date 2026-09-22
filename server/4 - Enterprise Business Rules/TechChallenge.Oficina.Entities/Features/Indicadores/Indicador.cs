using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Entities.Features.Indicadores;

public sealed class Indicador
{
    public int Id { get; private set; }
    public TimeSpan TempoMedioExecucao { get; private set; }
    public TimeSpan TempoMedioEntrega { get; private set; }

    private Indicador()
    {
    }

    private Indicador(TimeSpan tempoMedioExecucao, TimeSpan tempoMedioEntrega)
    {
        Id = 1;
        Atualizar(tempoMedioExecucao, tempoMedioEntrega);
    }

    public static Indicador Criar(TimeSpan tempoMedioExecucao, TimeSpan tempoMedioEntrega)
    {
        return new Indicador(tempoMedioExecucao, tempoMedioEntrega);
    }

    public void Atualizar(TimeSpan tempoMedioExecucao, TimeSpan tempoMedioEntrega)
    {
        if (tempoMedioExecucao < TimeSpan.Zero)
        {
            throw new DomainException("O tempo medio de execucao da ordem de servico deve ser valido.");
        }

        if (tempoMedioEntrega < TimeSpan.Zero)
        {
            throw new DomainException("O tempo medio de entrega da ordem de servico deve ser valido.");
        }

        Id = 1;
        TempoMedioExecucao = tempoMedioExecucao;
        TempoMedioEntrega = tempoMedioEntrega;
    }
}
