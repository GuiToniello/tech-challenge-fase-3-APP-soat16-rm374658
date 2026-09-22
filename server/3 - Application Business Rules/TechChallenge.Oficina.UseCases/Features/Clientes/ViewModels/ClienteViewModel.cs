namespace TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;

public sealed class ClienteViewModel
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Identificacao { get; set; } = string.Empty;
    public string TipoIdentificacao { get; set; } = string.Empty;
    public string? Email { get; set; }
}
