using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class AddAgentClientVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestCase");

            migrationBuilder.AddColumn<string>(
                name: "ClientVersion",
                table: "Agents",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientVersion",
                table: "Agents");

            migrationBuilder.CreateTable(
                name: "TestCase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PipelineJobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgentIdentifier = table.Column<int>(type: "int", nullable: true),
                    Attempt = table.Column<int>(type: "int", nullable: false),
                    ExtendField = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Group = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsNewFailure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Workspace = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCase_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestCase_Jobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TestCase_AgentId",
                table: "TestCase",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCase_PipelineJobId",
                table: "TestCase",
                column: "PipelineJobId");
        }
    }
}
