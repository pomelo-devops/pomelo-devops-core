using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomelo.DevOps.Models.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Binaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Filename = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Binaries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LoginProviders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Body = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ClientSecret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginProviders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PipelineId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JobNumber = table.Column<long>(type: "bigint", nullable: false),
                    LogSet = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    AgentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<double>(type: "double", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ValueJson = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Extended = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TriggerProviders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderEntryUrl = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
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
                    table.PrimaryKey("PK_TriggerProviders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BinaryPartitions",
                columns: table => new
                {
                    BinaryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Partition = table.Column<int>(type: "int", nullable: false),
                    Bytes = table.Column<byte[]>(type: "longblob", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinaryPartitions", x => new { x.BinaryId, x.Partition });
                    table.ForeignKey(
                        name: "FK_BinaryPartitions_Binaries_BinaryId",
                        column: x => x.BinaryId,
                        principalTable: "Binaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtensionPath = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Binaries_IconId",
                        column: x => x.IconId,
                        principalTable: "Binaries",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Username = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProviderId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Salt = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: true),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: true),
                    AvatarId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Binaries_AvatarId",
                        column: x => x.AvatarId,
                        principalTable: "Binaries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_LoginProviders_LoginProviderId",
                        column: x => x.LoginProviderId,
                        principalTable: "LoginProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Addins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addins", x => new { x.Id, x.Path });
                    table.ForeignKey(
                        name: "FK_Addins_Projects_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AgentPools",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProjectId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentPools_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GalleryPackages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AuthorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ListInGallery = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageBinaryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Methods = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Downloads = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryPackages", x => new { x.Id, x.Version });
                    table.ForeignKey(
                        name: "FK_GalleryPackages_Binaries_IconId",
                        column: x => x.IconId,
                        principalTable: "Binaries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GalleryPackages_Binaries_PackageBinaryId",
                        column: x => x.PackageBinaryId,
                        principalTable: "Binaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryPackages_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PersonalAccessTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Token = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalAccessTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pipelines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProjectId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    PipelineBody = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pipelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pipelines_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pipelines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    ProjectId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Agent = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TriggerType = table.Column<int>(type: "int", nullable: false),
                    PipelineId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<long>(type: "bigint", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TriggerName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    CurrentStageOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PipelineAccesses",
                columns: table => new
                {
                    PipelineId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccessType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineAccesses", x => new { x.PipelineId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PipelineAccesses_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PipelineFavorites",
                columns: table => new
                {
                    PipelineId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineFavorites", x => new { x.PipelineId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PipelineFavorites_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PipelineTriggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PipelineId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineTriggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineTriggers_Pipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "Pipelines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PipelineTriggers_TriggerProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "TriggerProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobLabels",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Label = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobLabels", x => new { x.JobId, x.Label });
                    table.ForeignKey(
                        name: "FK_JobLabels_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobVariables",
                columns: table => new
                {
                    PipelineJobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobVariables", x => new { x.PipelineJobId, x.Name });
                    table.ForeignKey(
                        name: "FK_JobVariables_Jobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgentPoolId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeartBeat = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Address = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentJobStageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_AgentPools_AgentPoolId",
                        column: x => x.AgentPoolId,
                        principalTable: "AgentPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PipelineJobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgentPoolId = table.Column<long>(type: "bigint", nullable: true),
                    IsolationLevel = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    ConditionVariable = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    Identifier = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobStages_AgentPools_AgentPoolId",
                        column: x => x.AgentPoolId,
                        principalTable: "AgentPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobStages_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_JobStages_Jobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PipelineJobId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Workspace = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Group = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AgentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AgentIdentifier = table.Column<int>(type: "int", nullable: true),
                    ExtendField = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Result = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Attempt = table.Column<int>(type: "int", nullable: false),
                    IsNewFailure = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCases_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestCases_Jobs_PipelineJobId",
                        column: x => x.PipelineJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JobSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Method = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PipelineJobStageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    ConditionVariable = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Timeout = table.Column<int>(type: "int", nullable: false),
                    Retry = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    ArgumentsString = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorHandlingMode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSteps_JobStages_PipelineJobStageId",
                        column: x => x.PipelineJobStageId,
                        principalTable: "JobStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Addins_OrganizationId",
                table: "Addins",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentPools_ProjectId",
                table: "AgentPools",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentPoolId",
                table: "Agents",
                column: "AgentPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CurrentJobStageId",
                table: "Agents",
                column: "CurrentJobStageId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryPackages_AuthorId",
                table: "GalleryPackages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryPackages_IconId",
                table: "GalleryPackages",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryPackages_ListInGallery",
                table: "GalleryPackages",
                column: "ListInGallery");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryPackages_PackageBinaryId",
                table: "GalleryPackages",
                column: "PackageBinaryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLabels_Label",
                table: "JobLabels",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PipelineId_Number",
                table: "Jobs",
                columns: new[] { "PipelineId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TriggeredAt",
                table: "Jobs",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_AgentId",
                table: "JobStages",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_AgentPoolId",
                table: "JobStages",
                column: "AgentPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_PipelineJobId",
                table: "JobStages",
                column: "PipelineJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobStages_Status",
                table: "JobStages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobSteps_PipelineJobStageId",
                table: "JobSteps",
                column: "PipelineJobStageId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginProviders_Enabled_Priority",
                table: "LoginProviders",
                columns: new[] { "Enabled", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginProviders_Mode",
                table: "LoginProviders",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_PipelineId_JobNumber_LogSet_Time",
                table: "Logs",
                columns: new[] { "PipelineId", "JobNumber", "LogSet", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Timestamp_AgentId_Type",
                table: "Metrics",
                columns: new[] { "Timestamp", "AgentId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_Token",
                table: "PersonalAccessTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_Token_ExpireAt",
                table: "PersonalAccessTokens",
                columns: new[] { "Token", "ExpireAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_UserId",
                table: "PersonalAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineAccesses_UserId",
                table: "PipelineAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineFavorites_UserId",
                table: "PipelineFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_ProjectId",
                table: "Pipelines",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_UserId",
                table: "Pipelines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_Visibility",
                table: "Pipelines",
                column: "Visibility");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTriggers_Enabled",
                table: "PipelineTriggers",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTriggers_PipelineId",
                table: "PipelineTriggers",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineTriggers_ProviderId",
                table: "PipelineTriggers",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Order",
                table: "Policies",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserId",
                table: "ProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IconId",
                table: "Projects",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_AgentId",
                table: "TestCases",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_PipelineJobId_Workspace_AgentIdentifier",
                table: "TestCases",
                columns: new[] { "PipelineJobId", "Workspace", "AgentIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_TriggerProviders_Priority",
                table: "TriggerProviders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AvatarId",
                table: "Users",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LoginProviderId_Username",
                table: "Users",
                columns: new[] { "LoginProviderId", "Username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_JobStages_CurrentJobStageId",
                table: "Agents",
                column: "CurrentJobStageId",
                principalTable: "JobStages",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentPools_Projects_ProjectId",
                table: "AgentPools");

            migrationBuilder.DropForeignKey(
                name: "FK_Pipelines_Projects_ProjectId",
                table: "Pipelines");

            migrationBuilder.DropForeignKey(
                name: "FK_Agents_AgentPools_AgentPoolId",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_JobStages_AgentPools_AgentPoolId",
                table: "JobStages");

            migrationBuilder.DropForeignKey(
                name: "FK_Agents_JobStages_CurrentJobStageId",
                table: "Agents");

            migrationBuilder.DropTable(
                name: "Addins");

            migrationBuilder.DropTable(
                name: "BinaryPartitions");

            migrationBuilder.DropTable(
                name: "GalleryPackages");

            migrationBuilder.DropTable(
                name: "JobLabels");

            migrationBuilder.DropTable(
                name: "JobSteps");

            migrationBuilder.DropTable(
                name: "JobVariables");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Metrics");

            migrationBuilder.DropTable(
                name: "PersonalAccessTokens");

            migrationBuilder.DropTable(
                name: "PipelineAccesses");

            migrationBuilder.DropTable(
                name: "PipelineFavorites");

            migrationBuilder.DropTable(
                name: "PipelineTriggers");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "TestCases");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "TriggerProviders");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "AgentPools");

            migrationBuilder.DropTable(
                name: "JobStages");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Pipelines");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Binaries");

            migrationBuilder.DropTable(
                name: "LoginProviders");
        }
    }
}
