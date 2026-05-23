using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdictApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePrivacyFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideCommentsFromProfile",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HidePostsFromProfile",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideCommentsFromProfile",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HidePostsFromProfile",
                table: "AspNetUsers");
        }
    }
}
