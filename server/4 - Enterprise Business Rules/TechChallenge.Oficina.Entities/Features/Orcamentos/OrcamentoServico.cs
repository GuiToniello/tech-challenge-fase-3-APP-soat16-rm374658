using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.Entities.Features.Orcamentos;

public sealed class OrcamentoServico
{
    public Guid ServicoId { get; private set; }
    public string NomeServico { get; private set; }
    public decimal ValorTotal { get; private set; }

    private OrcamentoServico()
    {
        NomeServico = string.Empty;
    }

    private OrcamentoServico(Guid servicoId, string nomeServico, decimal valorTotal)
    {
        ServicoId = servicoId;
        NomeServico = nomeServico;
        ValorTotal = decimal.Round(valorTotal, 2, MidpointRounding.ToEven);
    }

    public static OrcamentoServico Criar(Servico servico)
    {
        if (servico is null)
        {
            throw new DomainException("O servico do orcamento e obrigatorio.");
        }

        var valorTotal = servico.ItensServico.Sum(item => item.Quantidade * item.Insumo.ValorUnitario);
        return new OrcamentoServico(servico.Id, servico.Nome, valorTotal);
    }
}
