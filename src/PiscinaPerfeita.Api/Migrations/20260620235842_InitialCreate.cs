using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "piscina-perfeita");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:piscina-perfeita.tipo_revestimento_enum", "Vinil,Alvenaria,Fibra de vidro,Pastilhas,Porcelanato/Ceramica,Pedras naturais")
                .Annotation("Npgsql:Enum:piscina-perfeita.tipo_tratamento_enum", "Quimico tradicional,Salinizacao,Ozonio,Luz ultravioleta")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Produtos",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "text", nullable: false),
                    unidademedida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    emmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    createtat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Piscinas",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    volumelitros = table.Column<decimal>(type: "numeric", nullable: true),
                    profundidademedia = table.Column<decimal>(type: "numeric", nullable: true),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_piscinas", x => x.id);
                    table.ForeignKey(
                        name: "fk_piscinas_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analises",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    piscinaid = table.Column<Guid>(type: "uuid", nullable: false),
                    dataanalise = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ph = table.Column<decimal>(type: "numeric", nullable: true),
                    clorolivre = table.Column<decimal>(type: "numeric", nullable: true),
                    alcalinidade = table.Column<decimal>(type: "numeric", nullable: true),
                    temperatura = table.Column<List<decimal>>(type: "numeric[]", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analises", x => x.id);
                    table.ForeignKey(
                        name: "fk_analises_piscinas_piscinaid",
                        column: x => x.piscinaid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    piscinaid = table.Column<Guid>(type: "uuid", nullable: false),
                    produtoid = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidadeatual = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_estoque_piscinas_piscinaid",
                        column: x => x.piscinaid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_estoque_produtos_produtoid",
                        column: x => x.produtoid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    piscinaid = table.Column<Guid>(type: "uuid", nullable: false),
                    produtoid = table.Column<Guid>(type: "uuid", nullable: false),
                    tipomovimentacao = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: true),
                    valor = table.Column<decimal>(type: "numeric", nullable: true),
                    datamovimentacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimentacoesestoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimentacoesestoque_piscinas_piscinaid",
                        column: x => x.piscinaid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_movimentacoesestoque_produtos_produtoid",
                        column: x => x.produtoid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analises_piscinaid",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "piscinaid");

            migrationBuilder.CreateIndex(
                name: "ix_estoque_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "piscinaid");

            migrationBuilder.CreateIndex(
                name: "ix_estoque_produtoid",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "produtoid");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoesestoque_piscinaid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "piscinaid");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoesestoque_produtoid",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "produtoid");

            migrationBuilder.CreateIndex(
                name: "ix_piscinas_usuarioid",
                schema: "piscina-perfeita",
                table: "Piscinas",
                column: "usuarioid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analises",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "Estoque",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "Piscinas",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "Produtos",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "piscina-perfeita");
        }
    }
}
