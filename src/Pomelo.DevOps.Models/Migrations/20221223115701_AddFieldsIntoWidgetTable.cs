using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class AddFieldsIntoWidgetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Widgets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Widgets",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Widgets");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Widgets");
        }
    }
}
