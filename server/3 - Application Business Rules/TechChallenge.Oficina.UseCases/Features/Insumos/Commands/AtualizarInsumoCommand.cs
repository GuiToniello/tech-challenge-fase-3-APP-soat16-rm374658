using System.ComponentModel.DataAnnotations;

namespace TechChallenge.Oficina.UseCases.Features.Insumos.Commands;

public sealed class AtualizarInsumoCommand
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O nome do insumo e obrigatorio.")]
    [MaxLength(150, ErrorMessage = "O nome do insumo deve ter no maximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O fabricante do insumo e obrigatorio.")]
    [MaxLength(150, ErrorMessage = "O fabricante deve ter no maximo 150 caracteres.")]
    public string Fabricante { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade disponivel nao pode ser negativa.")]
    public int QuantidadeDisponivel { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O valor unitario nao pode ser negativo.")]
    public decimal ValorUnitario { get; set; }
}
