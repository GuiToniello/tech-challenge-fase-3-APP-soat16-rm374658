using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Veiculos.VOs;

namespace TechChallenge.Oficina.Entities.Features.Veiculos;

public class Veiculo
{
    public Guid Id { get; private set; }
    public PlacaMercosul Placa { get; private set; } = default!;
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public string Renavam { get; private set; } = string.Empty;
    public Guid ClienteId { get; private set; }

    public static Veiculo Criar(string placa, string marca, string modelo, int ano, string renavam, Guid clienteId)
    {
        return new Veiculo(placa, marca, modelo, ano, renavam, clienteId);
    }

    private Veiculo()
    {
    }

    private Veiculo(string placa, string marca, string modelo, int ano, string renavam, Guid clienteId)
    {
        Id = Guid.NewGuid();
        AtualizarPlaca(placa);
        AtualizarMarca(marca);
        AtualizarModelo(modelo);
        AtualizarAno(ano);
        AtualizarRenavam(renavam);
        AtualizarClienteId(clienteId);
    }

    public void AtualizarPlaca(string placa)
    {
        Placa = PlacaMercosul.Criar(placa);
    }

    public void AtualizarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new DomainException("A marca do veículo é obrigatória.");
        }

        Marca = marca.Trim();
    }

    public void AtualizarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new DomainException("O modelo do veículo é obrigatório.");
        }

        Modelo = modelo.Trim();
    }

    public void AtualizarAno(int ano)
    {
        var anoAtual = DateTime.UtcNow.Year;

        if (ano < 1886 || ano > anoAtual + 1)
        {
            throw new DomainException($"O ano do veículo deve estar entre 1886 e {anoAtual + 1}.");
        }

        Ano = ano;
    }

    public void AtualizarRenavam(string renavam)
    {
        if (string.IsNullOrWhiteSpace(renavam))
        {
            throw new DomainException("O RENAVAM do veículo é obrigatório.");
        }

        var renavamNormalizado = renavam.Trim();

        if (renavamNormalizado.Length < 9 || renavamNormalizado.Length > 11 || !renavamNormalizado.All(char.IsDigit))
        {
            throw new DomainException("O RENAVAM informado é inválido.");
        }

        Renavam = renavamNormalizado;
    }

    public void AtualizarClienteId(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            throw new DomainException("O cliente do veículo é obrigatório.");
        }

        ClienteId = clienteId;
    }
}
