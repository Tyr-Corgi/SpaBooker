using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoSettingsToContentSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VideoAutoplay",
                table: "ContentSections",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "VideoLoop",
                table: "ContentSections",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "VideoShowControls",
                table: "ContentSections",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoAutoplay",
                table: "ContentSections");

            migrationBuilder.DropColumn(
                name: "VideoLoop",
                table: "ContentSections");

            migrationBuilder.DropColumn(
                name: "VideoShowControls",
                table: "ContentSections");
        }
    }
}

