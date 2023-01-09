using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class AddDashboardToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dashboard",
                table: "Projects",
                type: "json",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dashboard",
                table: "Projects");
        }
    }
}
