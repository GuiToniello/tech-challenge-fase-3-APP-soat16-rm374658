using System.Text;
using System.Text.RegularExpressions;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Entities.Features.Veiculos.VOs;

public readonly struct PlacaMercosul : IEquatable<PlacaMercosul>
{
    private static readonly Regex FormatoMercosul = new(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public string Valor { get; }

    private PlacaMercosul(string valor)
    {
        Valor = valor;
    }

    public static PlacaMercosul Criar(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            throw new DomainException("A placa do veículo é obrigatória.");
        }

        var normalizada = Normalizar(valorInformado);

        if (!EhPlacaMercosul(normalizada))
        {
            throw new DomainException("A placa informada deve estar no padrão Mercosul (ex: ABC1D23).");
        }

        return new PlacaMercosul(normalizada);
    }

    public static bool EhValido(string valorInformado)
    {
        if (string.IsNullOrWhiteSpace(valorInformado))
        {
            return false;
        }

        return EhPlacaMercosul(Normalizar(valorInformado));
    }

    public override string ToString() => Valor;

    public bool Equals(PlacaMercosul other) => Valor == other.Valor;

    public override bool Equals(object? obj) => obj is PlacaMercosul other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode(StringComparison.Ordinal);

    private static string Normalizar(string valor)
    {
        var caracteres = valor.Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return new string(caracteres);
    }

    private static bool EhPlacaMercosul(string valor) => FormatoMercosul.IsMatch(valor);
}
