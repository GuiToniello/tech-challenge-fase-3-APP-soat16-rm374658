using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.Entities.Features.OrdensServico.VOs;

public sealed class HistoricoStatusOrdemServico : IEquatable<HistoricoStatusOrdemServico>
{
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataAlteracao { get; private set; }

    private HistoricoStatusOrdemServico()
    {
    }

    private HistoricoStatusOrdemServico(StatusOrdemServico status, DateTime dataAlteracao)
    {
        Status = status;
        DataAlteracao = dataAlteracao;
    }

    public static HistoricoStatusOrdemServico Criar(StatusOrdemServico status, DateTime dataAlteracao)
    {
        if (dataAlteracao == default)
        {
            throw new DomainException("A data do historico de status da ordem de servico e obrigatoria.");
        }

        return new HistoricoStatusOrdemServico(status, dataAlteracao);
    }

    public bool Equals(HistoricoStatusOrdemServico? other)
    {
        return other is not null && Status == other.Status && DataAlteracao == other.DataAlteracao;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HistoricoStatusOrdemServico);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Status, DataAlteracao);
    }
}
