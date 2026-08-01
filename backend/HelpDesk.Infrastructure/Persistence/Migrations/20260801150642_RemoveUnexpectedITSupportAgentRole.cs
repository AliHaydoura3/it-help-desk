using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnexpectedITSupportAgentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE userRole
                FROM [AspNetUserRoles] AS userRole
                INNER JOIN [AspNetRoles] AS role ON role.[Id] = userRole.[RoleId]
                WHERE role.[NormalizedName] = 'ITSUPPORTAGENT';

                DELETE FROM [AspNetRoles]
                WHERE [NormalizedName] = 'ITSUPPORTAGENT';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM [AspNetRoles]
                    WHERE [NormalizedName] = 'ITSUPPORTAGENT'
                )
                INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                VALUES (NEWID(), 'ITSupportAgent', 'ITSUPPORTAGENT', CONVERT(nvarchar(36), NEWID()));
                """);
        }
    }
}
