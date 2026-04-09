using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemIntelligencePlatform.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedIntelligenceFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClusterId",
                table: "AppLogEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContainsPii",
                table: "AppLogEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "AppIncidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ContainsPii",
                table: "AppIncidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MergedIntoIncidentId",
                table: "AppIncidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppAlertHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    FiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAlertHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefinitionJson = table.Column<string>(type: "text", nullable: false),
                    SeverityOverride = table.Column<int>(type: "integer", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAlertRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCopilotConversationMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCopilotConversationMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLogClusters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignatureHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogClusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLogSourceConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SettingsJson = table.Column<string>(type: "text", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogSourceConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMergedIncidentLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CanonicalIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MergedIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMergedIncidentLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMetricSamples",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    TagsJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMetricSamples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybookRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlaybookRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TriggerDefinitionJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlaybooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSavedLogSearches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FilterJson = table.Column<string>(type: "text", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSavedLogSearches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybookRunSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaybookRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlaybookRunSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPlaybookRunSteps_AppPlaybookRuns_PlaybookRunId",
                        column: x => x.PlaybookRunId,
                        principalTable: "AppPlaybookRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybookSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlaybookSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPlaybookSteps_AppPlaybooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "AppPlaybooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogEvents_ClusterId",
                table: "AppLogEvents",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLogEvents_ContainsPii",
                table: "AppLogEvents",
                column: "ContainsPii");

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_AssignedUserId",
                table: "AppIncidents",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_ContainsPii",
                table: "AppIncidents",
                column: "ContainsPii");

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_MergedIntoIncidentId",
                table: "AppIncidents",
                column: "MergedIntoIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlertHistories_AlertRuleId",
                table: "AppAlertHistories",
                column: "AlertRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlertHistories_ApplicationId",
                table: "AppAlertHistories",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlertHistories_FiredAt",
                table: "AppAlertHistories",
                column: "FiredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlertRules_ApplicationId",
                table: "AppAlertRules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAlertRules_IsEnabled",
                table: "AppAlertRules",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppCopilotConversationMessages_IncidentId",
                table: "AppCopilotConversationMessages",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLogClusters_ApplicationId",
                table: "AppLogClusters",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLogClusters_SignatureHash",
                table: "AppLogClusters",
                column: "SignatureHash");

            migrationBuilder.CreateIndex(
                name: "IX_AppLogSourceConfigurations_IsEnabled",
                table: "AppLogSourceConfigurations",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppMergedIncidentLinks_CanonicalIncidentId",
                table: "AppMergedIncidentLinks",
                column: "CanonicalIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMergedIncidentLinks_MergedIncidentId",
                table: "AppMergedIncidentLinks",
                column: "MergedIncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMetricSamples_ApplicationId_Name_Timestamp",
                table: "AppMetricSamples",
                columns: new[] { "ApplicationId", "Name", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMetricSamples_Timestamp",
                table: "AppMetricSamples",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookRuns_IncidentId",
                table: "AppPlaybookRuns",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookRuns_PlaybookId",
                table: "AppPlaybookRuns",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookRunSteps_PlaybookRunId",
                table: "AppPlaybookRunSteps",
                column: "PlaybookRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookSteps_PlaybookId",
                table: "AppPlaybookSteps",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSavedLogSearches_UserId",
                table: "AppSavedLogSearches",
                column: "UserId");

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_AppLogEvents_Message_fts" ON "AppLogEvents"
                USING gin (to_tsvector('english', coalesce("Message", '')));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_AppLogEvents_Message_fts"";");

            migrationBuilder.DropTable(
                name: "AppAlertHistories");

            migrationBuilder.DropTable(
                name: "AppAlertRules");

            migrationBuilder.DropTable(
                name: "AppCopilotConversationMessages");

            migrationBuilder.DropTable(
                name: "AppLogClusters");

            migrationBuilder.DropTable(
                name: "AppLogSourceConfigurations");

            migrationBuilder.DropTable(
                name: "AppMergedIncidentLinks");

            migrationBuilder.DropTable(
                name: "AppMetricSamples");

            migrationBuilder.DropTable(
                name: "AppPlaybookRunSteps");

            migrationBuilder.DropTable(
                name: "AppPlaybookSteps");

            migrationBuilder.DropTable(
                name: "AppSavedLogSearches");

            migrationBuilder.DropTable(
                name: "AppPlaybookRuns");

            migrationBuilder.DropTable(
                name: "AppPlaybooks");

            migrationBuilder.DropIndex(
                name: "IX_AppLogEvents_ClusterId",
                table: "AppLogEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppLogEvents_ContainsPii",
                table: "AppLogEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppIncidents_AssignedUserId",
                table: "AppIncidents");

            migrationBuilder.DropIndex(
                name: "IX_AppIncidents_ContainsPii",
                table: "AppIncidents");

            migrationBuilder.DropIndex(
                name: "IX_AppIncidents_MergedIntoIncidentId",
                table: "AppIncidents");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "AppLogEvents");

            migrationBuilder.DropColumn(
                name: "ContainsPii",
                table: "AppLogEvents");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "AppIncidents");

            migrationBuilder.DropColumn(
                name: "ContainsPii",
                table: "AppIncidents");

            migrationBuilder.DropColumn(
                name: "MergedIntoIncidentId",
                table: "AppIncidents");
        }
    }
}
