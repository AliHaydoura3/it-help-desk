using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingLifecycleMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAtUtc",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAtUtc",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE ticket
                SET ResolvedAtUtc = resolution.ResolvedAtUtc
                FROM Tickets AS ticket
                CROSS APPLY
                (
                    SELECT MIN(history.OccurredAtUtc) AS ResolvedAtUtc
                    FROM TicketHistories AS history
                    WHERE history.TicketId = ticket.Id
                      AND history.Action = 'Status changed'
                      AND history.NewValue IN ('Resolved', 'Closed')
                ) AS resolution
                WHERE resolution.ResolvedAtUtc IS NOT NULL;

                UPDATE ticket
                SET ClosedAtUtc = closure.ClosedAtUtc
                FROM Tickets AS ticket
                CROSS APPLY
                (
                    SELECT MIN(history.OccurredAtUtc) AS ClosedAtUtc
                    FROM TicketHistories AS history
                    WHERE history.TicketId = ticket.Id
                      AND history.NewValue = 'Closed'
                ) AS closure
                WHERE closure.ClosedAtUtc IS NOT NULL;

                UPDATE Tickets
                SET ClosedAtUtc = UpdatedAtUtc
                WHERE IsCancelled = 1 AND ClosedAtUtc IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResolvedAtUtc",
                table: "Tickets",
                column: "ResolvedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_ResolvedAtUtc",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ResolvedAtUtc",
                table: "Tickets");
        }
    }
}
