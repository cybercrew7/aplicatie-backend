using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace saxafon.Migrations
{
    /// <inheritdoc />
    public partial class Migration19Nov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.RenameColumn(
                name: "Building_id",
                table: "Resources",
                newName: "buildingId");

            migrationBuilder.AddColumn<DateTime>(
                name: "profile_image_updated_at",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_image_url",
                table: "Users",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Resources",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "Resources",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_booking_minutes",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_bookings_per_user_per_week",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_concurrent_bookings",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "min_booking_minutes",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "Resources",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "approved_by_user_id",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expected_attendees",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_or_cancel_reason",
                table: "Reservations",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reviewed_at",
                table: "Reservations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_approved_by_user_id",
                table: "Reservations",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ResourceId_start_time_end_time",
                table: "Reservations",
                columns: new[] { "ResourceId", "start_time", "end_time" });

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_approved_by_user_id",
                table: "Reservations",
                column: "approved_by_user_id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
          
            migrationBuilder.DropIndex(
                name: "IX_Reservations_ResourceId",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_approved_by_user_id",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_approved_by_user_id",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ResourceId_start_time_end_time",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "profile_image_updated_at",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "profile_image_url",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "max_booking_minutes",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "max_bookings_per_user_per_week",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "max_concurrent_bookings",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "min_booking_minutes",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "type",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "approved_by_user_id",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "expected_attendees",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "rejection_or_cancel_reason",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "reviewed_at",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "buildingId",
                table: "Resources",
                newName: "Building_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "Resources",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ResourceId",
                table: "Reservations",
                column: "ResourceId");
        }
    }
}
