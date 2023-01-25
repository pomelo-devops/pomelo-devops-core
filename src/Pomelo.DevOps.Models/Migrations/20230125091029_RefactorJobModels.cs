using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class RefactorJobModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiagramWorkflowInstanceId",
                table: "Jobs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DiagramWorkflowInstanceId",
                table: "Jobs",
                column: "DiagramWorkflowInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_WorkflowInstances_DiagramWorkflowInstanceId",
                table: "Jobs",
                column: "DiagramWorkflowInstanceId",
                principalTable: "WorkflowInstances",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_WorkflowInstances_DiagramWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_DiagramWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DiagramWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Jobs");
        }
    }
}
