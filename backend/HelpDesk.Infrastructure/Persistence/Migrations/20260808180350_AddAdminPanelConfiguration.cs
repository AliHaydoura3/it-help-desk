using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HelpDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPanelConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AutomaticAssignmentEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MaximumOpenTicketsPerEmployee = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketCategorySettings",
                columns: table => new
                {
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCategorySettings", x => x.Category);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "AutomaticAssignmentEnabled", "EmailNotificationsEnabled", "MaximumOpenTicketsPerEmployee", "OrganizationName", "SupportEmail", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { 1, true, true, 25, "IT Help Desk", "support@example.com", new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "TicketCategorySettings",
                columns: new[] { "Category", "Description", "DisplayName", "IsActive", "SortOrder", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { "AccessRequest", "Accounts, permissions, and resource access.", "Access request", true, 50, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { "Email", "Mailbox, delivery, calendar, and email client issues.", "Email", true, 40, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { "Hardware", "Physical devices, peripherals, and equipment.", "Hardware", true, 10, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { "Network", "Connectivity, VPN, Wi-Fi, and network access.", "Network", true, 30, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { "Other", "Support requests that do not match another category.", "Other", true, 60, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { "Software", "Applications, operating systems, and licensing.", "Software", true, 20, new DateTime(2026, 8, 8, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketCategorySettings_IsActive_SortOrder",
                table: "TicketCategorySettings",
                columns: new[] { "IsActive", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TicketCategorySettings");
        }
    }
}
