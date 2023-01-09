using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class RefactorTestCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Agents_AgentId",
                table: "TestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Jobs_PipelineJobId",
                table: "TestCases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_TestCases_PipelineJobId_Workspace_AgentIdentifier",
                table: "TestCases");

            migrationBuilder.RenameTable(
                name: "TestCases",
                newName: "TestCase");

            migrationBuilder.RenameIndex(
                name: "IX_TestCases_AgentId",
                table: "TestCase",
                newName: "IX_TestCase_AgentId");

            migrationBuilder.AddColumn<string>(
                name: "JobExtensionId",
                table: "PipelineTriggers",
                type: "varchar(64)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCase",
                table: "TestCase",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "JobExtensions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExtensionEntryUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ViewUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManageUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExtensions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTriggers_JobExtensionId",
                table: "PipelineTriggers",
                column: "JobExtensionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCase_PipelineJobId",
                table: "TestCase",
                column: "PipelineJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExtensions_Priority",
                table: "JobExtensions",
                column: "Priority");

            migrationBuilder.AddForeignKey(
                name: "FK_PipelineTriggers_JobExtensions_JobExtensionId",
                table: "PipelineTriggers",
                column: "JobExtensionId",
                principalTable: "JobExtensions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCase_Agents_AgentId",
                table: "TestCase",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCase_Jobs_PipelineJobId",
                table: "TestCase",
                column: "PipelineJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PipelineTriggers_JobExtensions_JobExtensionId",
                table: "PipelineTriggers");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCase_Agents_AgentId",
                table: "TestCase");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCase_Jobs_PipelineJobId",
                table: "TestCase");

            migrationBuilder.DropTable(
                name: "JobExtensions");

            migrationBuilder.DropIndex(
                name: "IX_PipelineTriggers_JobExtensionId",
                table: "PipelineTriggers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCase",
                table: "TestCase");

            migrationBuilder.DropIndex(
                name: "IX_TestCase_PipelineJobId",
                table: "TestCase");

            migrationBuilder.DropColumn(
                name: "JobExtensionId",
                table: "PipelineTriggers");

            migrationBuilder.RenameTable(
                name: "TestCase",
                newName: "TestCases");

            migrationBuilder.RenameIndex(
                name: "IX_TestCase_AgentId",
                table: "TestCases",
                newName: "IX_TestCases_AgentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCases",
                table: "TestCases",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_PipelineJobId_Workspace_AgentIdentifier",
                table: "TestCases",
                columns: new[] { "PipelineJobId", "Workspace", "AgentIdentifier" });

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Agents_AgentId",
                table: "TestCases",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Jobs_PipelineJobId",
                table: "TestCases",
                column: "PipelineJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
