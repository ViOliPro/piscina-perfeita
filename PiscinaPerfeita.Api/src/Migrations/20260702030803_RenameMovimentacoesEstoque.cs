using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameMovimentacoesEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoesestoque_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoesestoque_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoesestoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropPrimaryKey(
                name: "pk_movimentacoesestoque",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque");

            migrationBuilder.RenameTable(
                name: "MovimentacoesEstoque",
                schema: "piscina-perfeita",
                newName: "Movimentacoes",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoesestoque_usuarioid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                newName: "ix_movimentacoes_usuarioid");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoesestoque_produtoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                newName: "ix_movimentacoes_produtoid");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoesestoque_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                newName: "ix_movimentacoes_piscinaid");

            migrationBuilder.AlterColumn<string>(
                name: "valor",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_movimentacoes",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "produtoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Produtos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
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
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_movimentacoes",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.RenameTable(
                name: "Movimentacoes",
                schema: "piscina-perfeita",
                newName: "MovimentacoesEstoque",
                newSchema: "piscina-perfeita");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoes_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                newName: "ix_movimentacoesestoque_usuarioid");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoes_produtoid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                newName: "ix_movimentacoesestoque_produtoid");

            migrationBuilder.RenameIndex(
                name: "ix_movimentacoes_piscinaid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                newName: "ix_movimentacoesestoque_piscinaid");

            migrationBuilder.AlterColumn<decimal>(
                name: "valor",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "pk_movimentacoesestoque",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoesestoque_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoesestoque_produtos_produtoid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "produtoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Produtos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoesestoque_usuarios_usuarioid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "usuarioid",
                principalSchema: "piscina-perfeita",
                principalTable: "Usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
