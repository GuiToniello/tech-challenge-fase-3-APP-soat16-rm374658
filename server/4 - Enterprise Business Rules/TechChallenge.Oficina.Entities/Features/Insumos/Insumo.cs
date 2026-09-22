using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Entities.Features.Insumos;

public class Insumo
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Fabricante { get; private set; } = string.Empty;
    public int QuantidadeDisponivel { get; private set; }
    public decimal ValorUnitario { get; private set; }

    public static Insumo Criar(string nome, string fabricante, int quantidadeDisponivel, decimal valorUnitario)
    {
        return new Insumo(nome, fabricante, quantidadeDisponivel, valorUnitario);
    }

    private Insumo()
    {
    }

    private Insumo(string nome, string fabricante, int quantidadeDisponivel, decimal valorUnitario)
    {
        Id = Guid.NewGuid();
        AtualizarNome(nome);
        AtualizarFabricante(fabricante);
        AtualizarQuantidadeDisponivel(quantidadeDisponivel);
        AtualizarValorUnitario(valorUnitario);
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do insumo é obrigatório.");
        }

        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length < 3)
        {
            throw new DomainException("O nome do insumo deve possuir ao menos 3 caracteres.");
        }

        if (nomeNormalizado.Length > 150)
        {
            throw new DomainException("O nome do insumo deve ter no máximo 150 caracteres.");
        }

        Nome = nomeNormalizado;
    }

    public void AtualizarFabricante(string fabricante)
    {
        if (string.IsNullOrWhiteSpace(fabricante))
        {
            throw new DomainException("O fabricante do insumo é obrigatório.");
        }

        var fabricanteNormalizado = fabricante.Trim();

        if (fabricanteNormalizado.Length > 150)
        {
            throw new DomainException("O fabricante do insumo deve ter no máximo 150 caracteres.");
        }

        Fabricante = fabricanteNormalizado;
    }

    public void AtualizarQuantidadeDisponivel(int quantidadeDisponivel)
    {
        if (quantidadeDisponivel < 0)
        {
            throw new DomainException("A quantidade disponível do insumo não pode ser negativa.");
        }

        QuantidadeDisponivel = quantidadeDisponivel;
    }

    public void AtualizarValorUnitario(decimal valorUnitario)
    {
        if (valorUnitario < 0)
        {
            throw new DomainException("O valor unitário do insumo não pode ser negativo.");
        }

        ValorUnitario = decimal.Round(valorUnitario, 2, MidpointRounding.ToEven);
    }

    public void VerificarDisponibilidade(int quantidadeNecessaria)
    {
        if (quantidadeNecessaria <= 0)
        {
            throw new DomainException("A quantidade necessaria deve ser maior que zero.");
        }

        if (QuantidadeDisponivel < quantidadeNecessaria)
        {
            throw new DomainException($"Estoque insuficiente do insumo '{Nome}'. Disponivel: {QuantidadeDisponivel}, Necessario: {quantidadeNecessaria}.");
        }
    }

    public void DebitarEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade a debitar deve ser maior que zero.");
        }

        if (QuantidadeDisponivel < quantidade)
        {
            throw new DomainException($"Estoque insuficiente para debito do insumo '{Nome}'. Disponivel: {QuantidadeDisponivel}, Solicitado: {quantidade}.");
        }

        AtualizarQuantidadeDisponivel(QuantidadeDisponivel - quantidade);
    }
}
