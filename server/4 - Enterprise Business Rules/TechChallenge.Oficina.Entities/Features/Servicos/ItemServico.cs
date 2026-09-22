using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.Entities.Features.Servicos;

public class ItemServico
{
    public Guid ServicoId { get; private set; }
    public Servico? Servico { get; private set; }
    public Guid InsumoId { get; private set; }
    public Insumo Insumo { get; private set; } = null!;
    public int Quantidade { get; private set; }

    public static ItemServico Criar(Insumo insumo, int quantidade)
    {
        return new ItemServico(insumo, quantidade);
    }

    private ItemServico()
    {
    }

    private ItemServico(Insumo insumo, int quantidade)
    {
        DefinirInsumo(insumo);
        AtualizarQuantidade(quantidade);
    }

    public void AtualizarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade do item de servico deve ser maior que zero.");
        }

        Quantidade = quantidade;
    }

    internal void VincularAoServico(Servico servico)
    {
        if (servico is null)
        {
            throw new DomainException("O servico do item de servico e obrigatorio.");
        }

        Servico = servico;
        ServicoId = servico.Id;
    }

    private void DefinirInsumo(Insumo insumo)
    {
        if (insumo is null)
        {
            throw new DomainException("O insumo do item de servico e obrigatorio.");
        }

        Insumo = insumo;
        InsumoId = insumo.Id;
    }
}
