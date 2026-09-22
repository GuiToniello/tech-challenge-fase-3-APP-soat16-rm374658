namespace TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;

public sealed class ServicoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public IReadOnlyCollection<ItemServicoViewModel> ItensServico { get; set; } = [];
}
