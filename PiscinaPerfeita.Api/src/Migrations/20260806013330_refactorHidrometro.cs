using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class refactorHidrometro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_hidrometro_localid",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.DropColumn(
                name: "consumo",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.DropColumn(
                name: "criadoem",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "dataleitura",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "leituraatual",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "observacoes",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_hidrometro_localid_dataleitura",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                columns: new[] { "localid", "dataleitura" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_hidrometro_localid_dataleitura",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.DropColumn(
                name: "dataleitura",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.DropColumn(
                name: "leituraatual",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.DropColumn(
                name: "observacoes",
                schema: "piscina-perfeita",
                table: "Hidrometro");

            migrationBuilder.AddColumn<float>(
                name: "consumo",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "criadoem",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.CreateIndex(
                name: "ix_hidrometro_localid",
                schema: "piscina-perfeita",
                table: "Hidrometro",
                column: "localid");
        }
    }
}
