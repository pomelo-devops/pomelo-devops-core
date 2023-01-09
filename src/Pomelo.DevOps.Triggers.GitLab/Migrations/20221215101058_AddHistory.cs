using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Triggers.GitLab.Migrations
{
    public partial class AddHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TriggerHistories",
                columns: table => new
                {
                    NamespaceProject = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CommitHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerHistories", x => new { x.NamespaceProject, x.CommitHash });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriggerHistories");
        }
    }
}
