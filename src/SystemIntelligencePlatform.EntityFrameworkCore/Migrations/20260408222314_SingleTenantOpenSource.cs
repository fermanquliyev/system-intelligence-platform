using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemIntelligencePlatform.Migrations
{
    /// <inheritdoc />
    public partial class SingleTenantOpenSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpTenantConnectionStrings");

            migrationBuilder.DropTable(
                name: "AppMonthlyUsages");

            migrationBuilder.DropTable(
                name: "AppSubscriptions");

            migrationBuilder.DropTable(
                name: "AbpTenants");

            migrationBuilder.DropIndex(
                name: "IX_AppWebhookRegistrations_TenantId",
                table: "AppWebhookRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_AppMonitoredApplications_TenantId_Name",
                table: "AppMonitoredApplications");

            migrationBuilder.DropIndex(
                name: "IX_AppLogEvents_TenantId",
                table: "AppLogEvents");

            migrationBuilder.DropIndex(
                name: "IX_AppIncidents_TenantId",
                table: "AppIncidents");

            migrationBuilder.DropIndex(
                name: "IX_AppFailedLogEvents_TenantId",
                table: "AppFailedLogEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppWebhookRegistrations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppMonitoredApplications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppLogEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppIncidents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppIncidentComments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AppFailedLogEvents");

            migrationBuilder.CreateIndex(
                name: "IX_AppMonitoredApplications_Name",
                table: "AppMonitoredApplications",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppMonitoredApplications_Name",
                table: "AppMonitoredApplications");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppWebhookRegistrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppMonitoredApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppLogEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppIncidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppIncidentComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppFailedLogEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AbpTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMonthlyUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AiCallsUsed = table.Column<int>(type: "integer", nullable: false),
                    LogsIngested = table.Column<long>(type: "bigint", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMonthlyUsages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenantConnectionStrings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenantConnectionStrings", x => new { x.TenantId, x.Name });
                    table.ForeignKey(
                        name: "FK_AbpTenantConnectionStrings_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWebhookRegistrations_TenantId",
                table: "AppWebhookRegistrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMonitoredApplications_TenantId_Name",
                table: "AppMonitoredApplications",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLogEvents_TenantId",
                table: "AppLogEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIncidents_TenantId",
                table: "AppIncidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFailedLogEvents_TenantId",
                table: "AppFailedLogEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_Name",
                table: "AbpTenants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_NormalizedName",
                table: "AbpTenants",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AppMonthlyUsages_TenantId_Month",
                table: "AppMonthlyUsages",
                columns: new[] { "TenantId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSubscriptions_StripeSubscriptionId",
                table: "AppSubscriptions",
                column: "StripeSubscriptionId",
                unique: true,
                filter: "\"StripeSubscriptionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSubscriptions_TenantId",
                table: "AppSubscriptions",
                column: "TenantId",
                unique: true);
        }
    }
}
