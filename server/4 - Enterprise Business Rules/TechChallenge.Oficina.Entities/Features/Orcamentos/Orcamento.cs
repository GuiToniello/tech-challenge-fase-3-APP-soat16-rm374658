using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.Entities.Features.Orcamentos;

public sealed class Orcamento
{
    private readonly List<OrcamentoServico> _servicos = [];

    public Guid OrdemServicoId { get; private set; }
    public DateTime DataGeracao { get; private set; }
    public decimal ValorTotal { get; private set; }
    public IReadOnlyCollection<OrcamentoServico> Servicos => _servicos.AsReadOnly();

    private Orcamento()
    {
    }

    private Orcamento(Guid ordemServicoId, IReadOnlyCollection<Servico> servicos, DateTime dataGeracao)
    {
        if (ordemServicoId == Guid.Empty)
        {
            throw new DomainException("A ordem de servico do orcamento e obrigatoria.");
        }

        if (dataGeracao == default)
        {
            throw new DomainException("A data de geracao do orcamento e obrigatoria.");
        }

        OrdemServicoId = ordemServicoId;
        DataGeracao = dataGeracao;

        DefinirServicos(servicos);
    }

    public static Orcamento Criar(Guid ordemServicoId, IReadOnlyCollection<Servico> servicos, DateTime? dataGeracao = null)
    {
        return new Orcamento(ordemServicoId, servicos, dataGeracao ?? DateTime.UtcNow);
    }

    private void DefinirServicos(IReadOnlyCollection<Servico> servicos)
    {
        if (servicos is null || servicos.Count == 0)
        {
            throw new DomainException("A ordem de servico deve possuir ao menos um servico para gerar orcamento.");
        }

        _servicos.Clear();

        var servicosIdsAdicionados = new HashSet<Guid>();
        _servicos.AddRange(servicos
            .Where(servico => servicosIdsAdicionados.Add(servico.Id))
            .Select(OrcamentoServico.Criar));

        ValorTotal = decimal.Round(_servicos.Sum(item => item.ValorTotal), 2, MidpointRounding.ToEven);
    }
}
