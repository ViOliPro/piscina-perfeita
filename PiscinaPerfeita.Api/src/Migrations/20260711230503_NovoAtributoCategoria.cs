using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class NovoAtributoCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "categoria",
                schema: "piscina-perfeita",
                table: "Produtos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "categoria",
                schema: "piscina-perfeita",
                table: "Produtos");
        }
    }
}
