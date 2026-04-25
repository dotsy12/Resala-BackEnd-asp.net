using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_AspNetUsers_UserId",
                table: "StaffMembers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DeliveryAreas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeliveryAreas",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryAreas",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "DeliveryAreas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "DeliveryAreas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePublicId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "Address", "Governorate", "PasswordHash", "ProfileImagePublicId" },
                values: new object[] { null, null, "AQAAAAIAAYagAAAAEIW8lWqrJjfmgCRfamYY5TcFLxLdYDR6r66605bPgdixbtb88ip66Ujq269VqjZ76w==", null });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_SubscriptionId",
                table: "PaymentRequests",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAreas_Name_Governorate_City",
                table: "DeliveryAreas",
                columns: new[] { "Name", "Governorate", "City" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_SponsorshipSubscriptions_SubscriptionId",
                table: "PaymentRequests",
                column: "SubscriptionId",
                principalTable: "SponsorshipSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_AspNetUsers_UserId",
                table: "StaffMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_SponsorshipSubscriptions_SubscriptionId",
                table: "PaymentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_AspNetUsers_UserId",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_SubscriptionId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryAreas_Name_Governorate_City",
                table: "DeliveryAreas");

            migrationBuilder.DropColumn(
                name: "City",
                table: "DeliveryAreas");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "DeliveryAreas");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImagePublicId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DeliveryAreas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "DeliveryAreas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryAreas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOUzgdDNUhw1MxOSGFufHgfVuuc2u30rWJ/OpqVf14/xa9vaHe2SYq/iiKlfixfpPQ==");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_AspNetUsers_UserId",
                table: "StaffMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
