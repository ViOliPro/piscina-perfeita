using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class TableDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "depositoid",
                schema: "piscina-perfeita",
                table: "Estoques",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Depositos",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "text", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    localid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_depositos", x => x.id);
                    table.ForeignKey(
                        name: "fk_depositos_locais_localid",
                        column: x => x.localid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Locais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_estoques_depositoid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "depositoid");

            migrationBuilder.CreateIndex(
                name: "ix_depositos_localid",
                schema: "piscina-perfeita",
                table: "Depositos",
                column: "localid");

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_depositos_depositoid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "depositoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Depositos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_estoques_depositos_depositoid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropTable(
                name: "Depositos",
                schema: "piscina-perfeita");

            migrationBuilder.DropIndex(
                name: "ix_estoques_depositoid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "depositoid",
                schema: "piscina-perfeita",
                table: "Estoques");
        }
    }
}
