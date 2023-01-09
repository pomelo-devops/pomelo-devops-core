using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Triggers.Scheduled.Migrations
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
                    PomeloDevOpsProject = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PomeloDevOpsPipeline = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ArgumentsJson = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    JobNameTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    JobDescriptionTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    Hour = table.Column<int>(type: "INTEGER", nullable: false),
                    Minute = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triggers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Enabled",
                table: "Triggers",
                column: "Enabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triggers");
        }
    }
}
