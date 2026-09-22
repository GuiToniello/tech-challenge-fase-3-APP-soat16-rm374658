namespace TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;

public sealed class ItemServicoViewModel
{
    public Guid InsumoId { get; set; }
    public string InsumoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
