using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceGroupRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ServiceGroups",
                type: "text",
                nullable: false,
                defaultValue: "General");

            migrationBuilder.CreateTable(
                name: "ServiceGroupRooms",
                columns: table => new
                {
                    ServiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGroupRooms", x => new { x.ServiceGroupId, x.RoomId });
                    table.ForeignKey(
                        name: "FK_ServiceGroupRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceGroupRooms_ServiceGroups_ServiceGroupId",
                        column: x => x.ServiceGroupId,
                        principalTable: "ServiceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGroupRooms_RoomId",
                table: "ServiceGroupRooms",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceGroupRooms");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ServiceGroups");
        }
    }
}
