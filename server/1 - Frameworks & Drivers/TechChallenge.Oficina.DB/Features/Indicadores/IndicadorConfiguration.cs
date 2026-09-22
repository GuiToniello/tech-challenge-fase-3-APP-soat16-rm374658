using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Indicadores;

namespace TechChallenge.Oficina.DB.Data.Features.Indicadores;

public sealed class IndicadorConfiguration : IEntityTypeConfiguration<Indicador>
{
    public void Configure(EntityTypeBuilder<Indicador> builder)
    {
        builder.ToTable("indicadores");

        builder.HasKey(indicador => indicador.Id);

        builder.Property(indicador => indicador.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(indicador => indicador.TempoMedioExecucao)
            .HasColumnName("tempo_medio_execucao")
            .HasColumnType("interval")
            .IsRequired();

        builder.Property(indicador => indicador.TempoMedioEntrega)
            .HasColumnName("tempo_medio_entrega")
            .HasColumnType("interval")
            .IsRequired();
    }
}
