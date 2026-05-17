using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdictApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drafts_UserId",
                table: "Drafts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Drafts_UserId",
                table: "Drafts",
                column: "UserId",
                unique: true);
        }
    }
}
