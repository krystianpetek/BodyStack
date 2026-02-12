using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BodyStack.Server.Migrations
{
    /// <summary>
    /// Adds index on FitatuSessions.UpdatedAt for optimized GetLatestAsync queries.
    /// This index significantly improves performance of the "ORDER BY UpdatedAt DESC" query.
    /// </summary>
    public partial class AddFitatuSessionUpdatedAtIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FitatuSessions_UpdatedAt",
                table: "FitatuSessions",
                column: "UpdatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FitatuSessions_UpdatedAt",
                table: "FitatuSessions");
        }
    }
}
