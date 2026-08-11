using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tokenhash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refreshtoken", x => x.id);
                    table.ForeignKey(
                        name: "fk_refreshtoken_usuarios_usuarioid",
                        column: x => x.usuario_id,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refreshtoken_tokenhash",
                schema: "piscina-perfeita",
                table: "RefreshToken",
                column: "tokenhash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refreshtoken_usuarioid",
                schema: "piscina-perfeita",
                table: "RefreshToken",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "piscina-perfeita");
        }
    }
}
