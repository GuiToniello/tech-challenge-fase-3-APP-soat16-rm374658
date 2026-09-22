namespace TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;

public sealed class InsumoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int QuantidadeDisponivel { get; set; }
    public decimal ValorUnitario { get; set; }
}
