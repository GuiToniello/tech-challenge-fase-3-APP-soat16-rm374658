using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Entities.Features.Clientes.VOs;

public readonly struct Cnpj : IEquatable<Cnpj>
{
    private static readonly int[] PesosPrimeiroDigitoCnpj = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosSegundoDigitoCnpj = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public string Valor { get; }

    private Cnpj(string valor)
    {
        Valor = valor;
    }

    public static Cnpj Criar(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            throw new DomainException("A identificação do cliente é obrigatória.");
        }

        var cnpj = new Cnpj(Normalizar(valorInformado));
        if (!EhCnpj(cnpj.Valor))
        {
            throw new DomainException("A identificação informada deve ser um CNPJ válido.");
        }

        return cnpj;
    }

    public override string ToString() => Valor;

    public bool Equals(Cnpj other) => Valor == other.Valor;

    public override bool Equals(object? obj) => obj is Cnpj other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode(StringComparison.Ordinal);

    public static bool EhValido(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            return false;
        }

        return EhCnpj(Normalizar(valorInformado));
    }

    private static string Normalizar(string valor)
    {
        return new string(valor
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    private static bool EhCnpj(string valor)
    {
        if (valor.Length != 14 || TodosCaracteresIguais(valor))
        {
            return false;
        }

        if (!char.IsDigit(valor[12]) || !char.IsDigit(valor[13]))
        {
            return false;
        }

        var primeiroDigito = CalcularDigitoCnpj(valor.AsSpan(0, 12), PesosPrimeiroDigitoCnpj);
        var segundoDigito = CalcularDigitoCnpj(valor.AsSpan(0, 13), PesosSegundoDigitoCnpj);

        return valor[12] - '0' == primeiroDigito && valor[13] - '0' == segundoDigito;
    }

    private static int CalcularDigitoCnpj(ReadOnlySpan<char> valor, ReadOnlySpan<int> pesos)
    {
        var soma = 0;

        for (var indice = 0; indice < valor.Length; indice++)
        {
            soma += ConverterCaractereEmValor(valor[indice]) * pesos[indice];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int ConverterCaractereEmValor(char caractere)
    {
        if (char.IsDigit(caractere))
        {
            return caractere - '0';
        }

        if (char.IsLetter(caractere))
        {
            return caractere - 48;
        }

        throw new DomainException("A identificação do cliente contém caracteres inválidos.");
    }

    private static bool TodosCaracteresIguais(string valor)
    {
        return valor.Length > 0 && valor.All(caractere => caractere == valor[0]);
    }
}
