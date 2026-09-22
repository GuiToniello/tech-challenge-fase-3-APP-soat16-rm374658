using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Insumos;

namespace TechChallenge.Oficina.DB.Data.Features.Insumos;

public sealed class InsumoConfiguration : IEntityTypeConfiguration<Insumo>
{
    public void Configure(EntityTypeBuilder<Insumo> builder)
    {
        builder.ToTable("insumos");

        builder.HasKey(insumo => insumo.Id);

        builder.Property(insumo => insumo.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(insumo => insumo.Fabricante)
            .HasColumnName("fabricante")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(insumo => insumo.QuantidadeDisponivel)
            .HasColumnName("quantidade_disponivel")
            .IsRequired();

        builder.Property(insumo => insumo.ValorUnitario)
            .HasColumnName("valor_unitario")
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
