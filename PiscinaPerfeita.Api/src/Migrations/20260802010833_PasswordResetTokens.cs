using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class PasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "securitystamp",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    tokenhash = table.Column<string>(type: "text", nullable: false),
                    expiraem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usadoem = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criadoem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_passwordresettokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_passwordresettokens_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_passwordresettokens_tokenhash",
                schema: "piscina-perfeita",
                table: "PasswordResetTokens",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_passwordresettokens_usuarioid",
                schema: "piscina-perfeita",
                table: "PasswordResetTokens",
                column: "usuarioid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "piscina-perfeita");

            migrationBuilder.DropColumn(
                name: "securitystamp",
                schema: "piscina-perfeita",
                table: "Usuarios");
        }
    }
}
