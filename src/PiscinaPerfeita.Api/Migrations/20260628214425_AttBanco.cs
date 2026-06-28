using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AttBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_estoque_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_estoque_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "fk_estoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.DropPrimaryKey(
                name: "pk_estoque",
                schema: "piscina-perfeita",
                table: "Estoque");

            migrationBuilder.RenameTable(
                name: "usuarios",
                schema: "piscina-perfeita",
                newName: "Usuarios",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameTable(
                name: "Estoque",
                schema: "piscina-perfeita",
                newName: "Estoques",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameIndex(
                name: "ix_estoque_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoques",
                newName: "ix_estoques_usuarioid");

            migrationBuilder.RenameIndex(
                name: "ix_estoque_produtoid",
                schema: "piscina-perfeita",
                table: "Estoques",
                newName: "ix_estoques_produtoid");

            migrationBuilder.RenameIndex(
                name: "ix_estoque_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                newName: "ix_estoques_piscinaid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_estoques",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "produtoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Produtos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "usuarioid",
                principalSchema: "piscina-perfeita",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "fk_estoques_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "fk_estoques_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropPrimaryKey(
                name: "pk_estoques",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                schema: "piscina-perfeita",
                newName: "usuarios",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameTable(
                name: "Estoques",
                schema: "piscina-perfeita",
                newName: "Estoque",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameIndex(
                name: "ix_estoques_usuarioid",
                schema: "piscina-perfeita",
                table: "Estoque",
                newName: "ix_estoque_usuarioid");

            migrationBuilder.RenameIndex(
                name: "ix_estoques_produtoid",
                schema: "piscina-perfeita",
                table: "Estoque",
                newName: "ix_estoque_produtoid");

            migrationBuilder.RenameIndex(
                name: "ix_estoques_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoque",
                newName: "ix_estoque_piscinaid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_estoque",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_estoque_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoque_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "produtoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Produtos",
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
        }
    }
}
