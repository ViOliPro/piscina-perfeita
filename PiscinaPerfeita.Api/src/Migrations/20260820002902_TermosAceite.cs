using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class TermosAceite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "termosaceitosem",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "termosaceitosversao",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "termosaceitosem",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "termosaceitosversao",
                schema: "piscina-perfeita",
                table: "Usuarios");
        }
    }
}
