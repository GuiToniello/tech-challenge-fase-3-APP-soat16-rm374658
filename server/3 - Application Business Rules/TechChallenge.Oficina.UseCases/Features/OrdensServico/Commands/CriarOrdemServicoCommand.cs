namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;

public sealed class CriarOrdemServicoCommand
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public IReadOnlyCollection<Guid> ServicoIds { get; set; } = [];
}
