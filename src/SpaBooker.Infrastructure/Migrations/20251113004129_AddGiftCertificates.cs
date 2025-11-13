using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiftCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    PurchasedByUserId = table.Column<string>(type: "text", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientPhone = table.Column<string>(type: "text", nullable: true),
                    PersonalMessage = table.Column<string>(type: "text", nullable: true),
                    IsRedeemed = table.Column<bool>(type: "boolean", nullable: false),
                    RedeemedByUserId = table.Column<string>(type: "text", nullable: true),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    StripeRefundId = table.Column<string>(type: "text", nullable: true),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RestrictedToLocationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftCertificates_AspNetUsers_PurchasedByUserId",
                        column: x => x.PurchasedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftCertificates_AspNetUsers_RedeemedByUserId",
                        column: x => x.RedeemedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiftCertificates_Locations_RestrictedToLocationId",
                        column: x => x.RestrictedToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GiftCertificateTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GiftCertificateId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RelatedBookingId = table.Column<int>(type: "integer", nullable: true),
                    PerformedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCertificateTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftCertificateTransactions_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiftCertificateTransactions_Bookings_RelatedBookingId",
                        column: x => x.RelatedBookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiftCertificateTransactions_GiftCertificates_GiftCertificat~",
                        column: x => x.GiftCertificateId,
                        principalTable: "GiftCertificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_Code",
                table: "GiftCertificates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_PurchasedByUserId",
                table: "GiftCertificates",
                column: "PurchasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_RedeemedByUserId",
                table: "GiftCertificates",
                column: "RedeemedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_RestrictedToLocationId",
                table: "GiftCertificates",
                column: "RestrictedToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificates_Status",
                table: "GiftCertificates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificateTransactions_GiftCertificateId",
                table: "GiftCertificateTransactions",
                column: "GiftCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificateTransactions_PerformedByUserId",
                table: "GiftCertificateTransactions",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCertificateTransactions_RelatedBookingId",
                table: "GiftCertificateTransactions",
                column: "RelatedBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftCertificateTransactions");

            migrationBuilder.DropTable(
                name: "GiftCertificates");
        }
    }
}
