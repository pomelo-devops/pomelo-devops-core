using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class AddJobWorkflowTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PipelineDiagramStageId",
                table: "JobStages",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PipelineWorkflowInstanceId",
                table: "Jobs",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "JobWorkflowStages",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WorkflowInstanceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkflowStages", x => new { x.JobId, x.WorkflowInstanceId });
                    table.ForeignKey(
                        name: "FK_JobWorkflowStages_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobWorkflowStages_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_PipelineDiagramStageId",
                table: "JobStages",
                column: "PipelineDiagramStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PipelineWorkflowInstanceId",
                table: "Jobs",
                column: "PipelineWorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkflowStages_WorkflowInstanceId",
                table: "JobWorkflowStages",
                column: "WorkflowInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_WorkflowInstances_PipelineWorkflowInstanceId",
                table: "Jobs",
                column: "PipelineWorkflowInstanceId",
                principalTable: "WorkflowInstances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobStages_PipelineDiagramStages_PipelineDiagramStageId",
                table: "JobStages",
                column: "PipelineDiagramStageId",
                principalTable: "PipelineDiagramStages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_WorkflowInstances_PipelineWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_JobStages_PipelineDiagramStages_PipelineDiagramStageId",
                table: "JobStages");

            migrationBuilder.DropTable(
                name: "JobWorkflowStages");

            migrationBuilder.DropIndex(
                name: "IX_JobStages_PipelineDiagramStageId",
                table: "JobStages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PipelineWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PipelineDiagramStageId",
                table: "JobStages");

            migrationBuilder.DropColumn(
                name: "PipelineWorkflowInstanceId",
                table: "Jobs");
        }
    }
}
