using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriarConviteTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConviteTokens",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    perfil = table.Column<int>(type: "integer", nullable: false),
                    localid = table.Column<Guid>(type: "uuid", nullable: true),
                    criadoporid = table.Column<Guid>(type: "uuid", nullable: false),
                    criadoporsuperadmin = table.Column<bool>(type: "boolean", nullable: false),
                    tokenhash = table.Column<string>(type: "text", nullable: false),
                    expiraem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usadoem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criadoem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convitetokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_convitetokens_tokenhash",
                schema: "piscina-perfeita",
                table: "ConviteTokens",
                column: "tokenhash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConviteTokens",
                schema: "piscina-perfeita");
        }
    }
}
