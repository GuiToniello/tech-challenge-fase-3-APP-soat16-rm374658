namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

public sealed class OrcamentoServicoViewModel
{
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
}
