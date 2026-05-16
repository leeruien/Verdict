using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdictApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDilemmaImagePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePaths",
                table: "Dilemmas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePaths",
                table: "Dilemmas");
        }
    }
}
