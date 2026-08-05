using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class Hidrometro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hidrometro",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    localid = table.Column<Guid>(type: "uuid", nullable: false),
                    consumo = table.Column<float>(type: "real", nullable: true),
                    criadoem = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hidrometro", x => x.id);
                    table.ForeignKey(
                        name: "fk_hidrometro_locais_localid",
                        column: x => x.localid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Locais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hidrometro_localid",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                column: "localid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hidrometro",
                schema: "piscina-perfeita");
        }
    }
}
