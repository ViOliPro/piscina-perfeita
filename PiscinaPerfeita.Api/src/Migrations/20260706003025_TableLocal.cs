using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class TableLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "valor",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Produtos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Piscinas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Estoques",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "quantidademinima",
                schema: "piscina-perfeita",
                table: "Estoques",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Analises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Locais",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    pais = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "text", nullable: true),
                    createtat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosLocal",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    localid = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    createtat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    perfil = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarioslocal", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuarioslocal_locais_localid",
                        column: x => x.localid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Locais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuarioslocal_usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalSchema: "piscina-perfeita",
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_localid",
                schema: "piscina-perfeita",
                table: "Produtos",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_piscinas_localid",
                schema: "piscina-perfeita",
                table: "Piscinas",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_movimentacoes_localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_estoques_localid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_analises_localid",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_usuarioslocal_localid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                column: "localid");

            migrationBuilder.CreateIndex(
                name: "ix_usuarioslocal_usuarioid",
                schema: "piscina-perfeita",
                table: "UsuariosLocal",
                column: "usuarioid");

            migrationBuilder.AddForeignKey(
                name: "fk_analises_locais_localid",
                schema: "piscina-perfeita",
                table: "Analises",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_locais_localid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_movimentacoes_locais_localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_piscinas_locais_localid",
                schema: "piscina-perfeita",
                table: "Piscinas",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_produtos_locais_localid",
                schema: "piscina-perfeita",
                table: "Produtos",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios",
                column: "localid",
                principalSchema: "piscina-perfeita",
                principalTable: "Locais",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_analises_locais_localid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.DropForeignKey(
                name: "fk_estoques_locais_localid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "fk_movimentacoes_locais_localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_piscinas_locais_localid",
                schema: "piscina-perfeita",
                table: "Piscinas");

            migrationBuilder.DropForeignKey(
                name: "fk_produtos_locais_localid",
                schema: "piscina-perfeita",
                table: "Produtos");

            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_locais_localid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "UsuariosLocal",
                schema: "piscina-perfeita");

            migrationBuilder.DropTable(
                name: "Locais",
                schema: "piscina-perfeita");

            migrationBuilder.DropIndex(
                name: "ix_usuarios_localid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "ix_produtos_localid",
                schema: "piscina-perfeita",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "ix_piscinas_localid",
                schema: "piscina-perfeita",
                table: "Piscinas");

            migrationBuilder.DropIndex(
                name: "ix_movimentacoes_localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropIndex(
                name: "ix_estoques_localid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropIndex(
                name: "ix_analises_localid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Movimentacoes");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "quantidademinima",
                schema: "piscina-perfeita",
                table: "Estoques");

            migrationBuilder.DropColumn(
                name: "localid",
                schema: "piscina-perfeita",
                table: "Analises");

            migrationBuilder.AddColumn<string>(
                name: "valor",
                schema: "piscina-perfeita",
                table: "Movimentacoes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_estoques_piscinas_piscinaid",
                schema: "piscina-perfeita",
                table: "Estoques",
                column: "piscinaid",
                principalSchema: "piscina-perfeita",
                principalTable: "Piscinas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
