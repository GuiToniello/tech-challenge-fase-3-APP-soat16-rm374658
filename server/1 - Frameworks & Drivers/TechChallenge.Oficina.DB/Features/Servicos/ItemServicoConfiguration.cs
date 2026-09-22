using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.DB.Data.Features.Servicos;

public sealed class ItemServicoConfiguration : IEntityTypeConfiguration<ItemServico>
{
    public void Configure(EntityTypeBuilder<ItemServico> builder)
    {
        builder.ToTable("servicos_insumos");

        builder.HasKey(itemServico => new { itemServico.ServicoId, itemServico.InsumoId });

        builder.Property(itemServico => itemServico.ServicoId)
            .HasColumnName("servico_id")
            .IsRequired();

        builder.Property(itemServico => itemServico.InsumoId)
            .HasColumnName("insumo_id")
            .IsRequired();

        builder.Property(itemServico => itemServico.Quantidade)
            .HasColumnName("quantidade")
            .IsRequired();

        builder.HasOne(itemServico => itemServico.Servico)
            .WithMany(servico => servico.ItensServico)
            .HasForeignKey(itemServico => itemServico.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(itemServico => itemServico.Insumo)
            .WithMany()
            .HasForeignKey(itemServico => itemServico.InsumoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
