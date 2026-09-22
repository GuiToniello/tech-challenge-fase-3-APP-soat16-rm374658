using System.Net.Mail;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;

namespace TechChallenge.Oficina.Entities.Features.Clientes;

public class Cliente
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public IdentificacaoCliente Identificacao { get; private set; } = null!;
    public string? Email { get; private set; }

    public static Cliente Criar(string nomeCompleto, IdentificacaoCliente identificacao, string? email = null)
    {
        return new Cliente(nomeCompleto, identificacao, email);
    }

    private Cliente()
    {
    }

    private Cliente(string nomeCompleto, IdentificacaoCliente identificacao, string? email)
    {
        Id = Guid.NewGuid();
        AtualizarNomeCompleto(nomeCompleto);
        AtualizarIdentificacao(identificacao);
        AtualizarEmail(email);
    }

    public void AtualizarNomeCompleto(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            throw new DomainException("O nome completo do cliente é obrigatório.");
        }

        var nomeNormalizado = nomeCompleto.Trim();

        if (nomeNormalizado.Length < 3)
        {
            throw new DomainException("O nome completo do cliente deve possuir ao menos 3 caracteres.");
        }

        NomeCompleto = nomeNormalizado;
    }

    public void AtualizarIdentificacao(IdentificacaoCliente identificacao)
    {
        Identificacao = identificacao ?? throw new DomainException("A identificação do cliente é obrigatória.");
    }

    public void AtualizarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            Email = null;
            return;
        }

        var emailNormalizado = email.Trim();

        try
        {
            _ = new MailAddress(emailNormalizado);
        }
        catch (FormatException)
        {
            throw new DomainException("O email do cliente informado e invalido.");
        }

        Email = emailNormalizado;
    }
}
