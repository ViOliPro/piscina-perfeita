using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarioslocal_locais_localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal");

            migrationBuilder.AlterColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ultimolocalid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarioslocal_locais_localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarioslocal_locais_localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal");

            migrationBuilder.DropColumn(
                name: "cpf",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ultimolocalid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.AlterColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarioslocal_locais_localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
