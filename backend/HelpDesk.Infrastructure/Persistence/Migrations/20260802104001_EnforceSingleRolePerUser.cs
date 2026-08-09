using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleRolePerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH [RankedUserRoles] AS
                (
                    SELECT
                        userRole.[UserId],
                        userRole.[RoleId],
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY userRole.[UserId]
                            ORDER BY
                                CASE role.[NormalizedName]
                                    WHEN 'ADMIN' THEN 1
                                    WHEN 'ITSUPPORTSPECIALIST' THEN 2
                                    WHEN 'MANAGER' THEN 3
                                    WHEN 'EMPLOYEE' THEN 4
                                    ELSE 5
                                END,
                                userRole.[RoleId]
                        ) AS [RoleRank]
                    FROM [AspNetUserRoles] AS userRole
                    INNER JOIN [AspNetRoles] AS role ON role.[Id] = userRole.[RoleId]
                )
                DELETE userRole
                FROM [AspNetUserRoles] AS userRole
                INNER JOIN [RankedUserRoles] AS rankedRole
                    ON rankedRole.[UserId] = userRole.[UserId]
                    AND rankedRole.[RoleId] = userRole.[RoleId]
                WHERE rankedRole.[RoleRank] > 1;

                DECLARE @EmployeeRoleId uniqueidentifier =
                (
                    SELECT TOP(1) [Id]
                    FROM [AspNetRoles]
                    WHERE [NormalizedName] = 'EMPLOYEE'
                );

                IF @EmployeeRoleId IS NOT NULL
                BEGIN
                    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                    SELECT userAccount.[Id], @EmployeeRoleId
                    FROM [AspNetUsers] AS userAccount
                    WHERE NOT EXISTS
                    (
                        SELECT 1
                        FROM [AspNetUserRoles] AS userRole
                        WHERE userRole.[UserId] = userAccount.[Id]
                    );
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");
        }
    }
}
