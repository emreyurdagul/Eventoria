using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailMediaFileIdToMediaFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ThumbnailMediaFileId",
                table: "media_files",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_files_ThumbnailMediaFileId",
                table: "media_files",
                column: "ThumbnailMediaFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_media_files_media_files_ThumbnailMediaFileId",
                table: "media_files",
                column: "ThumbnailMediaFileId",
                principalTable: "media_files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_files_media_files_ThumbnailMediaFileId",
                table: "media_files");

            migrationBuilder.DropIndex(
                name: "IX_media_files_ThumbnailMediaFileId",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "ThumbnailMediaFileId",
                table: "media_files");
        }
    }
}
