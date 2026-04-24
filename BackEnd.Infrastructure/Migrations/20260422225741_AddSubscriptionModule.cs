using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Branch_Address",
                table: "PaymentRequests",
                newName: "Rep_Address");

            migrationBuilder.AddColumn<int>(
                name: "Branch_SlotId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rep_ContactName",
                table: "PaymentRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rep_ContactPhone",
                table: "PaymentRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderPhoneNumber",
                table: "PaymentRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchAppointmentSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenFrom = table.Column<TimeSpan>(type: "time", nullable: false),
                    OpenTo = table.Column<TimeSpan>(type: "time", nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    BookedCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchAppointmentSlots", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEF3zLqNHtpThiLB+ItJNn4y77Algucl+l+4UDf58QhKW4x8u6o2ikhBESVUYquJ5gw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchAppointmentSlots");

            migrationBuilder.DropColumn(
                name: "Branch_SlotId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "Rep_ContactName",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "Rep_ContactPhone",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "SenderPhoneNumber",
                table: "PaymentRequests");

            migrationBuilder.RenameColumn(
                name: "Rep_Address",
                table: "PaymentRequests",
                newName: "Branch_Address");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJdl54i0inuZ5qE/KBVBmaBxzThOQ+vifrRdu7A9GufWjk54jboWUwCZlUdz8lOPxA==");
        }
    }
}
