namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

public sealed class AcompanhamentoOrdemServicoViewModel
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public IReadOnlyCollection<HistoricoStatusOrdemServicoViewModel> HistoricoStatus { get; set; } = [];
}
