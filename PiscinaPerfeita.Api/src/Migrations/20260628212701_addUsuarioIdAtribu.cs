using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class addUsuarioIdAtribu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "datamovimentacao",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "Analises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoesestoque_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "ix_estoque_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "ix_analises_usuarioid",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "usuarioid");

            migrationBuilder.AddForeignKey(
                name: "fk_analises_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "usuarioid",
                principalSchema: "piscina-perfeita",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "usuarioid",
                principalSchema: "piscina-perfeita",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoesestoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "usuarioid",
                principalSchema: "piscina-perfeita",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_analises_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.DropForeignKey(
                name: "fk_estoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoesestoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoesestoque_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "ix_estoque_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropIndex(
                name: "ix_analises_usuarioid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.DropColumn(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "usuarioid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "datamovimentacao",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }
    }
}
