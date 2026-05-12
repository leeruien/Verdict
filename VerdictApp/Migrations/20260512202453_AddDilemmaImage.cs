using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdictApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDilemmaImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Dilemmas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Dilemmas");
        }
    }
}
