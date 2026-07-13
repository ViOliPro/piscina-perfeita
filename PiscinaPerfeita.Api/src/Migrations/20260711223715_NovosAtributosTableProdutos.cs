using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class NovosAtributosTableProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fabricante",
                schema: "piscina-perfeita",
                table: "Produtos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marca",
                schema: "piscina-perfeita",
                table: "Produtos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacoes",
                schema: "piscina-perfeita",
                table: "Produtos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fabricante",
                schema: "piscina-perfeita",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "marca",
                schema: "piscina-perfeita",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "observacoes",
                schema: "piscina-perfeita",
                table: "Produtos");
        }
    }
}
