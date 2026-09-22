namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

public sealed class OrdemServicoViewModel
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public IReadOnlyCollection<ServicoResumoOrdemServicoViewModel> Servicos { get; set; } = [];
    public OrcamentoViewModel? Orcamento { get; set; }
    public int Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
}
