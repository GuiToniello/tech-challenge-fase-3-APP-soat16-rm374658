using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes.Enums;

namespace TechChallenge.Oficina.Entities.Features.Clientes.VOs;

public sealed class IdentificacaoCliente : IEquatable<IdentificacaoCliente>
{
    private readonly Cpf? _cpf;
    private readonly Cnpj? _cnpj;

    public string Valor => _cpf?.Valor ?? _cnpj?.Valor ?? string.Empty;

    public TipoIdentificacaoCliente Tipo => _cpf.HasValue
        ? TipoIdentificacaoCliente.Cpf
        : TipoIdentificacaoCliente.Cnpj;

    private IdentificacaoCliente(string valor, TipoIdentificacaoCliente tipo)
    {
        switch (tipo)
        {
            case TipoIdentificacaoCliente.Cpf:
                _cpf = Cpf.Criar(valor);
                break;
            case TipoIdentificacaoCliente.Cnpj:
                _cnpj = Cnpj.Criar(valor);
                break;
        }
    }

    public static IdentificacaoCliente Criar(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            throw new DomainException("A identificação do cliente é obrigatória.");
        }

        if (Cpf.EhValido(valorInformado))
        {
            return new IdentificacaoCliente(valorInformado, TipoIdentificacaoCliente.Cpf);
        }

        if (Cnpj.EhValido(valorInformado))
        {
            return new IdentificacaoCliente(valorInformado, TipoIdentificacaoCliente.Cnpj);
        }

        throw new DomainException("A identificação informada deve ser um CPF ou CNPJ válido.");
    }

    public override string ToString()
    {
        return Valor;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IdentificacaoCliente);
    }

    public bool Equals(IdentificacaoCliente? other)
    {
        return other is not null && Valor == other.Valor && Tipo == other.Tipo;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Valor, Tipo);
    }
}
