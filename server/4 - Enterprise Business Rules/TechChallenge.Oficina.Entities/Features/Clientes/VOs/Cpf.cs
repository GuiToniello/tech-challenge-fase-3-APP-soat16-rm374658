using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Entities.Features.Clientes.VOs;

public readonly struct Cpf : IEquatable<Cpf>
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Cpf Criar(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            throw new DomainException("A identificação do cliente é obrigatória.");
        }

        var cpf = new Cpf(Normalizar(valorInformado));
        if (!EhCpf(cpf.Valor))
        {
            throw new DomainException("A identificação informada deve ser um CPF válido.");
        }

        return cpf;
    }

    public override string ToString() => Valor;

    public bool Equals(Cpf other) => Valor == other.Valor;

    public override bool Equals(object? obj) => obj is Cpf other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode(StringComparison.Ordinal);

    public static bool EhValido(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            return false;
        }

        return EhCpf(Normalizar(valorInformado));
    }

    private static string Normalizar(string valor)
    {
        return new string(valor
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    private static bool EhCpf(string valor)
    {
        if (valor.Length != 11 || valor.Any(c => !char.IsDigit(c)) || TodosCaracteresIguais(valor))
        {
            return false;
        }

        var primeiroDigito = CalcularDigitoCpf(valor.AsSpan(0, 9), 10);
        var segundoDigito = CalcularDigitoCpf(valor.AsSpan(0, 10), 11);

        return valor[9] - '0' == primeiroDigito && valor[10] - '0' == segundoDigito;
    }

    private static int CalcularDigitoCpf(ReadOnlySpan<char> valor, int pesoInicial)
    {
        var soma = 0;
        var peso = pesoInicial;

        foreach (var caractere in valor)
        {
            soma += (caractere - '0') * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static bool TodosCaracteresIguais(string valor)
    {
        return valor.Length > 0 && valor.All(caractere => caractere == valor[0]);
    }
}
