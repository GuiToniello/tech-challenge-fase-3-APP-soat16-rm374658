using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.VOs;

namespace TechChallenge.Oficina.DB.Data.Features.OrdensServico;

public sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    private const string OrdemServicoIdColumnName = "ordem_servico_id";

    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordens_servico");

        builder.HasKey(ordemServico => ordemServico.Id);

        builder.Property(ordemServico => ordemServico.ClienteId)
            .HasColumnName("cliente_id")
            .IsRequired();

        builder.Property(ordemServico => ordemServico.VeiculoId)
            .HasColumnName("veiculo_id")
            .IsRequired();

        builder.Property(ordemServico => ordemServico.Status)
            .HasColumnName("status")
            .HasColumnType("integer")
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(ordemServico => ordemServico.ClienteId);
        builder.HasIndex(ordemServico => ordemServico.VeiculoId);
        builder.HasIndex(ordemServico => ordemServico.Status);

        builder.HasOne<TechChallenge.Oficina.Entities.Features.Clientes.Cliente>()
            .WithMany()
            .HasForeignKey(ordemServico => ordemServico.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TechChallenge.Oficina.Entities.Features.Veiculos.Veiculo>()
            .WithMany()
            .HasForeignKey(ordemServico => ordemServico.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ordemServico => ordemServico.Servicos)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ordens_servico_servicos",
                right => right
                    .HasOne<TechChallenge.Oficina.Entities.Features.Servicos.Servico>()
                    .WithMany()
                    .HasForeignKey("servico_id")
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne<OrdemServico>()
                    .WithMany()
                    .HasForeignKey(OrdemServicoIdColumnName)
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("ordens_servico_servicos");
                    join.HasKey(OrdemServicoIdColumnName, "servico_id");
                });

        builder.OwnsOne(ordemServico => ordemServico.Orcamento, orcamentoBuilder =>
        {
            orcamentoBuilder.ToTable("ordens_servico_orcamentos");
            orcamentoBuilder.WithOwner().HasForeignKey(orcamento => orcamento.OrdemServicoId);
            orcamentoBuilder.HasKey(orcamento => orcamento.OrdemServicoId);

            orcamentoBuilder.Property(orcamento => orcamento.OrdemServicoId)
                .HasColumnName(OrdemServicoIdColumnName)
                .ValueGeneratedNever();

            orcamentoBuilder.Property(orcamento => orcamento.DataGeracao)
                .HasColumnName("data_geracao")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            orcamentoBuilder.Property(orcamento => orcamento.ValorTotal)
                .HasColumnName("valor_total")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            orcamentoBuilder.OwnsMany(orcamento => orcamento.Servicos, orcamentoServicoBuilder =>
            {
                orcamentoServicoBuilder.ToTable("ordens_servico_orcamentos_servicos");
                    orcamentoServicoBuilder.WithOwner().HasForeignKey(OrdemServicoIdColumnName);
                    orcamentoServicoBuilder.Property<int>("id").ValueGeneratedOnAdd();
                    orcamentoServicoBuilder.HasKey("id");

                orcamentoServicoBuilder.Property(orcamentoServico => orcamentoServico.ServicoId)
                    .HasColumnName("servico_id")
                    .IsRequired();

                orcamentoServicoBuilder.Property(orcamentoServico => orcamentoServico.NomeServico)
                    .HasColumnName("nome_servico")
                    .HasMaxLength(150)
                    .IsRequired();

                orcamentoServicoBuilder.Property(orcamentoServico => orcamentoServico.ValorTotal)
                    .HasColumnName("valor_total")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                orcamentoServicoBuilder.HasIndex(OrdemServicoIdColumnName);
                orcamentoServicoBuilder.HasIndex(orcamentoServico => orcamentoServico.ServicoId);
            });
        });

        builder.OwnsMany(ordemServico => ordemServico.HistoricoStatus, historicoBuilder =>
        {
            historicoBuilder.ToTable("ordens_servico_historico_status");
            historicoBuilder.WithOwner().HasForeignKey(OrdemServicoIdColumnName);
            historicoBuilder.Property<int>("id").ValueGeneratedOnAdd();
            historicoBuilder.HasKey("id");
            historicoBuilder.Property(historico => historico.Status)
                .HasColumnName("status")
                .HasColumnType("integer")
                .HasConversion<int>()
                .IsRequired();
            historicoBuilder.Property(historico => historico.DataAlteracao)
                .HasColumnName("data_alteracao")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            historicoBuilder.HasIndex(OrdemServicoIdColumnName);
            historicoBuilder.HasIndex(historico => historico.DataAlteracao);
        });
    }
}
