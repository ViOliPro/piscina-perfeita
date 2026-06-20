using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class EstadoInicialBanco : Migration
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    senhahash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Piscinas",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    VolumeLitros = table.Column<decimal>(type: "numeric", nullable: true),
                    ProfundidadeMedia = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piscinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Piscinas_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analises",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PiscinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataAnalise = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Ph = table.Column<decimal>(type: "numeric", nullable: true),
                    CloroLivre = table.Column<decimal>(type: "numeric", nullable: true),
                    Alcalinidade = table.Column<decimal>(type: "numeric", nullable: true),
                    Temperatura = table.Column<List<decimal>>(type: "numeric[]", nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analises_Piscinas_PiscinaId",
                        column: x => x.PiscinaId,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PiscinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantidadeAtual = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estoque_Piscinas_PiscinaId",
                        column: x => x.PiscinaId,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Estoque_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PiscinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoMovimentacao = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric", nullable: true),
                    DataMovimentacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Piscinas_PiscinaId",
                        column: x => x.PiscinaId,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Piscinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analises_PiscinaId",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "PiscinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_PiscinaId",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "PiscinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProdutoId",
                schema: "piscina-perfeita",
                table: "Estoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_PiscinaId",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "PiscinaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId",
                schema: "piscina-perfeita",
                table: "MovimentacoesEstoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Piscinas_UsuarioId",
                schema: "piscina-perfeita",
                table: "Piscinas",
                column: "UsuarioId");
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
                name: "usuarios");
        }
    }
}
