using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.Entities.Features.Servicos;

public class Servico
{
    private readonly List<ItemServico> _itensServico = [];

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public IReadOnlyCollection<ItemServico> ItensServico => _itensServico.AsReadOnly();

    public static Servico Criar(string nome, string descricao, IReadOnlyCollection<ItemServico>? itensServico)
    {
        return new Servico(nome, descricao, itensServico);
    }

    private Servico()
    {
    }

    private Servico(string nome, string descricao, IReadOnlyCollection<ItemServico>? itensServico)
    {
        Id = Guid.NewGuid();
        AtualizarNome(nome);
        AtualizarDescricao(descricao);
        DefinirItensServico(itensServico);
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do servico e obrigatorio.");
        }

        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length < 3)
        {
            throw new DomainException("O nome do servico deve possuir ao menos 3 caracteres.");
        }

        Nome = nomeNormalizado;
    }

    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new DomainException("A descricao do servico e obrigatoria.");
        }

        var descricaoNormalizada = descricao.Trim();

        if (descricaoNormalizada.Length < 3)
        {
            throw new DomainException("A descricao do servico deve possuir ao menos 3 caracteres.");
        }

        Descricao = descricaoNormalizada;
    }

    public void DefinirItensServico(IReadOnlyCollection<ItemServico>? itensServico)
    {
        _itensServico.Clear();

        if (itensServico is null)
        {
            return;
        }

        foreach (var itemServico in itensServico)
        {
            if (itemServico is null)
            {
                throw new DomainException("O item do servico e obrigatorio.");
            }

            if (_itensServico.All(item => item.InsumoId != itemServico.InsumoId))
            {
                itemServico.VincularAoServico(this);
                _itensServico.Add(itemServico);
            }
        }
    }
}
