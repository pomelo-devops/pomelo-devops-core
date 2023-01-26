using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class RefactorJobStageModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_WorkflowInstances_PipelineWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "JobWorkflowStages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PipelineWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "JobStages");

            migrationBuilder.DropColumn(
                name: "PipelineWorkflowInstanceId",
                table: "Jobs");

            migrationBuilder.CreateTable(
                name: "JobStageWorkflows",
                columns: table => new
                {
                    JobStageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WorkflowInstanceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    JobId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStageWorkflows", x => new { x.JobStageId, x.WorkflowInstanceId });
                    table.ForeignKey(
                        name: "FK_JobStageWorkflows_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobStageWorkflows_JobStages_JobStageId",
                        column: x => x.JobStageId,
                        principalTable: "JobStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobStageWorkflows_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageWorkflows_JobId",
                table: "JobStageWorkflows",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStageWorkflows_JobStageId",
                table: "JobStageWorkflows",
                column: "JobStageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobStageWorkflows_WorkflowInstanceId",
                table: "JobStageWorkflows",
                column: "WorkflowInstanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobStageWorkflows");

            migrationBuilder.AddColumn<int>(
                name: "Identifier",
                table: "JobStages",
                type: "int",
                nullable: true);

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
        }
    }
}
