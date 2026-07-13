using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AttDepositosMovimentacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.AlterColumn<Guid>(
                name: "piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "unidadelancamento",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AplicacoesProduto",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    localid = table.Column<Guid>(type: "uuid", nullable: false),
                    piscinaid = table.Column<Guid>(type: "uuid", nullable: false),
                    produtoid = table.Column<Guid>(type: "uuid", nullable: false),
                    depositoid = table.Column<Guid>(type: "uuid", nullable: false),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    analiseid = table.Column<Guid>(type: "uuid", nullable: true),
                    movimentacaoestoqueid = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unidadelancamento = table.Column<string>(type: "text", nullable: false),
                    dataaplicacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aplicacoesproduto", x => x.id);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_analises_analiseid",
                        column: x => x.analiseid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Analises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_depositos_depositoid",
                        column: x => x.depositoid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Depositos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_locais_localid",
                        column: x => x.localid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Locais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_movimentacoesestoque_movimentacaoestoqueid",
                        column: x => x.movimentacaoestoqueid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Movimentacoes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_piscinas_piscinaid",
                        column: x => x.piscinaid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_produtos_produtoid",
                        column: x => x.produtoid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aplicacoesproduto_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "depositoid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_analiseid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "analiseid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_depositoid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "depositoid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_localid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_movimentacaoestoqueid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "movimentacaoestoqueid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_piscinaid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "piscinaid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_produtoid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "produtoid");

            migrationBuilder.CreateIndex(
                name: "ix_aplicacoesproduto_usuarioid",
                schema: "piscina-perfeita",
                table: "AplicacoesProduto",
                column: "usuarioid");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_depositos_depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "depositoid",
                principalSchema: "piscina-perfeita",
                principalTable: "Depositos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_depositos_depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropTable(
                name: "AplicacoesProduto",
                schema: "piscina-perfeita");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoes_depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "depositoid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "unidadelancamento",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.AlterColumn<Guid>(
                name: "piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
