using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptImagePath",
                table: "PaymentRequests");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Sponsorships",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconPath",
                table: "Sponsorships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Sponsorships",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImagePublicId",
                table: "PaymentRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImageUrl",
                table: "PaymentRequests",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "EmergencyCases",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "EmergencyCases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEMvaY0lpQvsYMGBevtLfSCYRl4avXI2emT2y4OxFuW7BtW+2GLNanM6LGnTsVjDG/g==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Sponsorships");

            migrationBuilder.DropColumn(
                name: "ReceiptImagePublicId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "ReceiptImageUrl",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "EmergencyCases");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Sponsorships",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconPath",
                table: "Sponsorships",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptImagePath",
                table: "PaymentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "EmergencyCases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEF3zLqNHtpThiLB+ItJNn4y77Algucl+l+4UDf58QhKW4x8u6o2ikhBESVUYquJ5gw==");
        }
    }
}
