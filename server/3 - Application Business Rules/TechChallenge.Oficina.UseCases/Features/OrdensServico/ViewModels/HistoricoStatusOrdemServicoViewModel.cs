using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;

public sealed class HistoricoStatusOrdemServicoViewModel
{
    public StatusOrdemServico Status { get; set; }
    public DateTime DataAlteracao { get; set; }
}
