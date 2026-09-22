namespace TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;

public sealed class CriarVeiculoCommand
{
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Renavam { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
}
