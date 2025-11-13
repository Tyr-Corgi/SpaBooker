using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TherapistAvailability",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "MembershipCreditTransactions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TherapistAvailability");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "MembershipCreditTransactions");
        }
    }
}
