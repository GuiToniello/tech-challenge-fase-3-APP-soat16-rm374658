using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.DB.Data.Features.Servicos;

public sealed class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("servicos");

        builder.HasKey(servico => servico.Id);

        builder.Property(servico => servico.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(servico => servico.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasMany(servico => servico.ItensServico)
            .WithOne(itemServico => itemServico.Servico)
            .HasForeignKey(itemServico => itemServico.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
