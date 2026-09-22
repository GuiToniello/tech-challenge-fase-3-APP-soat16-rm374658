namespace TechChallenge.Oficina.UseCases.Features.Servicos.Commands;

public sealed class AtualizarServicoCommand
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public IReadOnlyCollection<ItemServicoCommand> ItensServico { get; set; } = [];
}
