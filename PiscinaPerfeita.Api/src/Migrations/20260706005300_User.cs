using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.AlterColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.AlterColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
