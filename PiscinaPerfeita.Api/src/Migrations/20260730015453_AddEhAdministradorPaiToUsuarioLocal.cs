using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEhAdministradorPaiToUsuarioLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ehadministradorpai",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ehadministradorpai",
                schema: "piscina-perfeita",
                table: "UsuariosLocal");
        }
    }
}
