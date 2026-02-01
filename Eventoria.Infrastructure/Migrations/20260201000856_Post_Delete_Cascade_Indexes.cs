using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Post_Delete_Cascade_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Caption = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "post_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_media_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_post_media_PostId_MediaFileId",
                table: "post_media",
                columns: new[] { "PostId", "MediaFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_media_PostId_Order",
                table: "post_media",
                columns: new[] { "PostId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_CreatedByUserId",
                table: "posts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_EventId",
                table: "posts",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.DropTable(
                name: "posts");
        }
    }
}
