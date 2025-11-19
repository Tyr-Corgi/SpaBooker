using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpaServices_LocationId",
                table: "SpaServices");

            migrationBuilder.DropIndex(
                name: "IX_MembershipCreditTransactions_UserMembershipId",
                table: "MembershipCreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GiftCertificateTransactions_GiftCertificateId",
                table: "GiftCertificateTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_LocationId",
                table: "Bookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClientNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    NoteType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientNotes_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StripeEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpaServices_LocationId_IsActive",
                table: "SpaServices",
                columns: new[] { "LocationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SpaServices_Name",
                table: "SpaServices",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_IsActive",
                table: "MembershipPlans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCreditTransactions_ExpiresAt",
                table: "MembershipCreditTransactions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCreditTransactions_UserMembershipId_CreatedAt",
                table: "MembershipCreditTransactions",
                columns: new[] { "UserMembershipId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificateTransactions_GiftCertificateId_CreatedAt",
                table: "GiftCertificateTransactions",
                columns: new[] { "GiftCertificateId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_ExpiresAt",
                table: "GiftCertificates",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_PurchasedByUserId_PurchasedAt",
                table: "GiftCertificates",
                columns: new[] { "PurchasedByUserId", "PurchasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_RecipientEmail",
                table: "GiftCertificates",
                column: "RecipientEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientId_StartTime",
                table: "Bookings",
                columns: new[] { "ClientId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LocationId_StartTime",
                table: "Bookings",
                columns: new[] { "LocationId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StripePaymentIntentId",
                table: "Bookings",
                column: "StripePaymentIntentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_EndTimeAfterStartTime",
                table: "Bookings",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_Timestamp",
                table: "AuditLogs",
                columns: new[] { "Action", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity",
                table: "AuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_ClientId_CreatedAt",
                table: "ClientNotes",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_ProcessedAt",
                table: "ProcessedWebhookEvents",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_StripeEventId",
                table: "ProcessedWebhookEvents",
                column: "StripeEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ClientNotes");

            migrationBuilder.DropTable(
                name: "ProcessedWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_SpaServices_LocationId_IsActive",
                table: "SpaServices");

            migrationBuilder.DropIndex(
                name: "IX_SpaServices_Name",
                table: "SpaServices");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_IsActive",
                table: "MembershipPlans");

            migrationBuilder.DropIndex(
                name: "IX_MembershipCreditTransactions_ExpiresAt",
                table: "MembershipCreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipCreditTransactions_UserMembershipId_CreatedAt",
                table: "MembershipCreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GiftCertificateTransactions_GiftCertificateId_CreatedAt",
                table: "GiftCertificateTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GiftCertificates_ExpiresAt",
                table: "GiftCertificates");

            migrationBuilder.DropIndex(
                name: "IX_GiftCertificates_PurchasedByUserId_PurchasedAt",
                table: "GiftCertificates");

            migrationBuilder.DropIndex(
                name: "IX_GiftCertificates_RecipientEmail",
                table: "GiftCertificates");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientId_StartTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_LocationId_StartTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StripePaymentIntentId",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_EndTimeAfterStartTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_SpaServices_LocationId",
                table: "SpaServices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCreditTransactions_UserMembershipId",
                table: "MembershipCreditTransactions",
                column: "UserMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificateTransactions_GiftCertificateId",
                table: "GiftCertificateTransactions",
                column: "GiftCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientId",
                table: "Bookings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LocationId",
                table: "Bookings",
                column: "LocationId");
        }
    }
}
