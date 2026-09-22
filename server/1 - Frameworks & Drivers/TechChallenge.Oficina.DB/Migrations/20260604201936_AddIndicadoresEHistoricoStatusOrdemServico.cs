using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TechChallenge.Oficina.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicadoresEHistoricoStatusOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indicadores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    tempo_medio_execucao = table.Column<TimeSpan>(type: "interval", nullable: false),
                    tempo_medio_entrega = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicadores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico_historico_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico_historico_status", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordens_servico_historico_status_ordens_servico_ordem_servic~",
                        column: x => x.ordem_servico_id,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_status",
                table: "ordens_servico",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_historico_status_data_alteracao",
                table: "ordens_servico_historico_status",
                column: "data_alteracao");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_historico_status_ordem_servico_id",
                table: "ordens_servico_historico_status",
                column: "ordem_servico_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indicadores");

            migrationBuilder.DropTable(
                name: "ordens_servico_historico_status");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_status",
                table: "ordens_servico");
        }
    }
}
