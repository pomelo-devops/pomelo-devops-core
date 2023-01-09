using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Triggers.GitLab.Migrations
{
    public partial class RefactorTriggers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Triggers_Enabled_GitLabNamespace_GitLabProject",
                table: "Triggers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TriggerHistories",
                table: "TriggerHistories");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Triggers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Triggers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TriggerHistories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriggerHistories",
                table: "TriggerHistories",
                columns: new[] { "Type", "NamespaceProject", "CommitHash" });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Enabled_Type_GitLabNamespace_GitLabProject",
                table: "Triggers",
                columns: new[] { "Enabled", "Type", "GitLabNamespace", "GitLabProject" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Triggers_Enabled_Type_GitLabNamespace_GitLabProject",
                table: "Triggers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TriggerHistories",
                table: "TriggerHistories");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TriggerHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TriggerHistories",
                table: "TriggerHistories",
                columns: new[] { "NamespaceProject", "CommitHash" });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Enabled_GitLabNamespace_GitLabProject",
                table: "Triggers",
                columns: new[] { "Enabled", "GitLabNamespace", "GitLabProject" });
        }
    }
}
