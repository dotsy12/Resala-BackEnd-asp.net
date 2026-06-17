using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedByStaffIdToPaymentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessedByStaffId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAL+Jj/jJJj3ePnl6thdkIxdSJYLOUv0yn6SFW6BdQ8pcAVXs5U41xa+HO1FNYrx8Q==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedByStaffId",
                table: "PaymentRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHYe/HTbT+X1vwOa5EDH6wBbDuMfzUzNRQQYb1J8UAIDMgRJXUQlHP8qPwdHW8Xf8A==");
        }
    }
}
