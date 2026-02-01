using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_20260201 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId1",
                table: "event_memberships",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_event_memberships_EventId1",
                table: "event_memberships",
                column: "EventId1");

            migrationBuilder.AddForeignKey(
                name: "FK_event_memberships_events_EventId1",
                table: "event_memberships",
                column: "EventId1",
                principalTable: "events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_memberships_events_EventId1",
                table: "event_memberships");

            migrationBuilder.DropIndex(
                name: "IX_event_memberships_EventId1",
                table: "event_memberships");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "event_memberships");
        }
    }
}
