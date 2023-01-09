using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Triggers.GitLab.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Triggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    GitLabNamespace = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    GitLabProject = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PomeloDevOpsProject = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PomeloDevOpsPipeline = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ArgumentsJson = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    JobNameTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    JobDescriptionTemplate = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triggers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Enabled_GitLabNamespace_GitLabProject",
                table: "Triggers",
                columns: new[] { "Enabled", "GitLabNamespace", "GitLabProject" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triggers");
        }
    }
}
