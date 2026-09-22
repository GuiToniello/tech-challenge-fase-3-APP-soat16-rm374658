using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TechChallenge.Oficina.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrcamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ordens_servico_orcamentos",
                columns: table => new
                {
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_geracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico_orcamentos", x => x.ordem_servico_id);
                    table.ForeignKey(
                        name: "FK_ordens_servico_orcamentos_ordens_servico_ordem_servico_id",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico_orcamentos_servicos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_servico = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico_orcamentos_servicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordens_servico_orcamentos_servicos_ordens_servico_orcamento~",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico_orcamentos",
                        principalColumn: "ordem_servico_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_orcamentos_servicos_ordem_servico_id",
                table: "ordens_servico_orcamentos_servicos",
                column: "ordem_servico_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_orcamentos_servicos_servico_id",
                table: "ordens_servico_orcamentos_servicos",
                column: "servico_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordens_servico_orcamentos_servicos");

            migrationBuilder.DropTable(
                name: "ordens_servico_orcamentos");
        }
    }
}
