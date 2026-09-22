namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;

public sealed class AbrirOrdemServicoCompletaCommand
{
    public ClienteAberturaOrdemServicoCommand Cliente { get; set; } = new();
    public VeiculoAberturaOrdemServicoCommand Veiculo { get; set; } = new();
    public IReadOnlyCollection<ServicoAberturaOrdemServicoCommand> Servicos { get; set; } = [];
}

public sealed class ClienteAberturaOrdemServicoCommand
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Identificacao { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public sealed class VeiculoAberturaOrdemServicoCommand
{
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Renavam { get; set; } = string.Empty;
}

public sealed class ServicoAberturaOrdemServicoCommand
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public IReadOnlyCollection<ItemServicoAberturaOrdemServicoCommand> ItensServico { get; set; } = [];
}

public sealed class ItemServicoAberturaOrdemServicoCommand
{
    public InsumoAberturaOrdemServicoCommand Insumo { get; set; } = new();
    public int Quantidade { get; set; }
}

public sealed class InsumoAberturaOrdemServicoCommand
{
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int QuantidadeDisponivel { get; set; }
    public decimal ValorUnitario { get; set; }
}
