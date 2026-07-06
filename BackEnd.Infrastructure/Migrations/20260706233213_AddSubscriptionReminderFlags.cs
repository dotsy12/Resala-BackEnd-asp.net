using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionReminderFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "SponsorshipSubscriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PostDueRemindersCount",
                table: "SponsorshipSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Sent100PercentReminder",
                table: "SponsorshipSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sent50PercentReminder",
                table: "SponsorshipSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Sent75PercentReminder",
                table: "SponsorshipSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOcEYVjgybLqp7nN+gohg4gwgy+q7KoO0fjjYowaSonYe/PDMo0iZoemC5xm2E1FLw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "SponsorshipSubscriptions");

            migrationBuilder.DropColumn(
                name: "PostDueRemindersCount",
                table: "SponsorshipSubscriptions");

            migrationBuilder.DropColumn(
                name: "Sent100PercentReminder",
                table: "SponsorshipSubscriptions");

            migrationBuilder.DropColumn(
                name: "Sent50PercentReminder",
                table: "SponsorshipSubscriptions");

            migrationBuilder.DropColumn(
                name: "Sent75PercentReminder",
                table: "SponsorshipSubscriptions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEPjSCuXmrleOJ5At+orZ7n5EWxnBTzOeIpc5jg7hqYUV0GQ46QLk7r7A9IAWGgnwiw==");
        }
    }
}
