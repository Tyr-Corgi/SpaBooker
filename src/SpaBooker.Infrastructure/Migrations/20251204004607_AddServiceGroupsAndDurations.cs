using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceGroupsAndDurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "Bookings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ServiceDurationId",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false, defaultValue: "General"),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    RequiresMembership = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceGroups_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDurations_ServiceGroups_ServiceGroupId",
                        column: x => x.ServiceGroupId,
                        principalTable: "ServiceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceGroupTherapists",
                columns: table => new
                {
                    ServiceGroupId = table.Column<int>(type: "integer", nullable: false),
                    TherapistId = table.Column<string>(type: "text", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGroupTherapists", x => new { x.ServiceGroupId, x.TherapistId });
                    table.ForeignKey(
                        name: "FK_ServiceGroupTherapists_AspNetUsers_TherapistId",
                        column: x => x.TherapistId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceGroupTherapists_ServiceGroups_ServiceGroupId",
                        column: x => x.ServiceGroupId,
                        principalTable: "ServiceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Bookings_ServiceDurationId",
                table: "Bookings",
                column: "ServiceDurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDurations_ServiceGroupId_IsActive",
                table: "ServiceDurations",
                columns: new[] { "ServiceGroupId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGroups_LocationId_IsActive",
                table: "ServiceGroups",
                columns: new[] { "LocationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGroups_Name",
                table: "ServiceGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGroupTherapists_TherapistId",
                table: "ServiceGroupTherapists",
                column: "TherapistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGroupRooms_RoomId",
                table: "ServiceGroupRooms",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceDurations_ServiceDurationId",
                table: "Bookings",
                column: "ServiceDurationId",
                principalTable: "ServiceDurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceDurations_ServiceDurationId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "ServiceDurations");

            migrationBuilder.DropTable(
                name: "ServiceGroupTherapists");

            migrationBuilder.DropTable(
                name: "ServiceGroupRooms");

            migrationBuilder.DropTable(
                name: "ServiceGroups");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ServiceDurationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceDurationId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
