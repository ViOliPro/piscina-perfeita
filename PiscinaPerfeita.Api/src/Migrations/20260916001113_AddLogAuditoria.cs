using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiscinaPerfeita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLogAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "piscina-perfeita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    occurredat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    localid = table.Column<Guid>(type: "uuid", nullable: true),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: true),
                    usuarioemail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entitytype = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    entityid = table.Column<Guid>(type: "uuid", nullable: true),
                    httpmethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ipaddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    useragent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    statuscode = table.Column<int>(type: "integer", nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    correlationid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditlogs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auditlogs_action_occurredat",
                schema: "piscina-perfeita",
                table: "AuditLogs",
                columns: new[] { "action", "occurredat" });

            migrationBuilder.CreateIndex(
                name: "ix_auditlogs_entitytype_entityid_occurredat",
                schema: "piscina-perfeita",
                table: "AuditLogs",
                columns: new[] { "entitytype", "entityid", "occurredat" });

            migrationBuilder.CreateIndex(
                name: "ix_auditlogs_localid_occurredat",
                schema: "piscina-perfeita",
                table: "AuditLogs",
                columns: new[] { "localid", "occurredat" });

            migrationBuilder.CreateIndex(
                name: "ix_auditlogs_usuarioid_occurredat",
                schema: "piscina-perfeita",
                table: "AuditLogs",
                columns: new[] { "usuarioid", "occurredat" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "piscina-perfeita");
        }
    }
}
