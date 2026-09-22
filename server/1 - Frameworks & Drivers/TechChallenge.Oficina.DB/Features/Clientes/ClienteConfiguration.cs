using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Clientes;

namespace TechChallenge.Oficina.DB.Data.Features.Clientes;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.NomeCompleto)
            .HasColumnName("nome_completo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cliente => cliente.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired(false);

        builder.OwnsOne(cliente => cliente.Identificacao, identificacao =>
        {
            identificacao.Property(valor => valor.Valor)
                .HasColumnName("identificacao")
                .HasMaxLength(14)
                .IsRequired();

            identificacao.Property(valor => valor.Tipo)
                .HasColumnName("tipo_identificacao")
                .HasColumnType("integer")
                .IsRequired();

            identificacao.WithOwner();
            identificacao.HasIndex(valor => valor.Valor)
                .IsUnique();
        });
    }
}
