using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpaBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl3 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ButtonText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ButtonLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LayoutType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BackgroundColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_DisplayOrder_IsActive",
                table: "ContentSections",
                columns: new[] { "DisplayOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentSections_IsActive",
                table: "ContentSections",
                column: "IsActive");

            // Seed example sections
            migrationBuilder.InsertData(
                table: "ContentSections",
                columns: new[] { "Title", "Subtitle", "Description", "SectionType", "ImageUrl", "LayoutType", "BackgroundColor", "DisplayOrder", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    {
                        "Experience Tranquility",
                        "A Sanctuary for Your Senses",
                        "Step into our serene spa environment where every detail has been carefully curated to provide the ultimate relaxation experience. Our treatment rooms feature soothing lighting, aromatherapy, and state-of-the-art amenities to ensure your complete comfort.",
                        "TextImage",
                        "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=1200",
                        "ImageLeft",
                        "white",
                        1,
                        true,
                        DateTime.UtcNow
                    },
                    {
                        "Expert Care",
                        "Skilled Hands, Healing Touch",
                        "Our licensed therapists bring years of experience and specialized training to every treatment. Each session is personalized to your unique needs, ensuring therapeutic benefits and deep relaxation. We are committed to your wellness journey.",
                        "TextImage",
                        "https://images.unsplash.com/photo-1519824145371-296894a0daa9?w=1200",
                        "ImageRight",
                        "cream",
                        2,
                        true,
                        DateTime.UtcNow
                    },
                    {
                        "Membership Benefits",
                        null,
                        "Join our exclusive membership program and enjoy priority booking, special rates, and monthly credits. Experience luxury spa treatments as part of your regular self-care routine.",
                        "FullWidthImage",
                        "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=1200",
                        "ImageCenter",
                        "white",
                        3,
                        true,
                        DateTime.UtcNow
                    },
                    {
                        "Our Facilities",
                        "Designed for Your Comfort",
                        "Explore our beautifully appointed spa featuring private treatment rooms, relaxation lounges, and premium amenities. Every space is designed to enhance your wellness experience.",
                        "Gallery",
                        "https://images.unsplash.com/photo-1596178060810-7d4d089e48e8?w=800",
                        "ImageCenter",
                        "cream",
                        4,
                        true,
                        DateTime.UtcNow
                    }
                });

            // Add ButtonText and ButtonLink for the sections that need them
            migrationBuilder.Sql(@"
                UPDATE ""ContentSections"" 
                SET ""ButtonText"" = 'View Services', ""ButtonLink"" = '/services' 
                WHERE ""Title"" = 'Experience Tranquility';
                
                UPDATE ""ContentSections"" 
                SET ""ButtonText"" = 'Meet Our Team', ""ButtonLink"" = '/therapists' 
                WHERE ""Title"" = 'Expert Care';
                
                UPDATE ""ContentSections"" 
                SET ""ButtonText"" = 'Learn More', ""ButtonLink"" = '/membership' 
                WHERE ""Title"" = 'Membership Benefits';
            ");

            // Add additional images for the gallery section
            migrationBuilder.Sql(@"
                UPDATE ""ContentSections"" 
                SET ""ImageUrl2"" = 'https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=800',
                    ""ImageUrl3"" = 'https://images.unsplash.com/photo-1600334129128-685c5582fd35?w=800'
                WHERE ""Title"" = 'Our Facilities';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentSections");
        }
    }
}


