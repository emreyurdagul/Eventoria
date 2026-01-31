using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventMembershipUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "participant_limit",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "photos_per_user_limit",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "videos_per_user_limit",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "event_admin_quotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxEvents = table.Column<int>(type: "integer", nullable: false),
                    MaxTotalParticipants = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_admin_quotas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_admin_quotas_AdminUserId",
                table: "event_admin_quotas",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_event_admin_quotas_AdminUserId_IsActive",
                table: "event_admin_quotas",
                columns: new[] { "AdminUserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_admin_quotas");

            migrationBuilder.DropColumn(
                name: "participant_limit",
                table: "events");

            migrationBuilder.DropColumn(
                name: "photos_per_user_limit",
                table: "events");

            migrationBuilder.DropColumn(
                name: "videos_per_user_limit",
                table: "events");
        }
    }
}
