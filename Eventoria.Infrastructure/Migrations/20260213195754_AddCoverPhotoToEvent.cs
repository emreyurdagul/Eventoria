using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverPhotoToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoverPhotoMediaFileId",
                table: "events",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverPhotoMediaFileId",
                table: "events");
        }
    }
}
