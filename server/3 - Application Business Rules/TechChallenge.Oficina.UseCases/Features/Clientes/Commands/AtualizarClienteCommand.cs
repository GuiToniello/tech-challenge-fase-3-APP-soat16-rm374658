namespace TechChallenge.Oficina.UseCases.Features.Clientes.Commands;

public sealed class AtualizarClienteCommand
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Identificacao { get; set; } = string.Empty;
    public string? Email { get; set; }
}
