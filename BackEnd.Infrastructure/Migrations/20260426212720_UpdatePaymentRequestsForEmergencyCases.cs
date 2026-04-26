using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentRequestsForEmergencyCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonorId",
                table: "PaymentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyCaseId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "PaymentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENfxCkd57hZ091ZIh5I39CpxlcucxqAnACgTN8q3HLLgrr1HwH9wsp6si6ZDihzxSA==");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_DonorId",
                table: "PaymentRequests",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_EmergencyCaseId",
                table: "PaymentRequests",
                column: "EmergencyCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Donors_DonorId",
                table: "PaymentRequests",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_EmergencyCases_EmergencyCaseId",
                table: "PaymentRequests",
                column: "EmergencyCaseId",
                principalTable: "EmergencyCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Donors_DonorId",
                table: "PaymentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_EmergencyCases_EmergencyCaseId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_DonorId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_EmergencyCaseId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "DonorId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "EmergencyCaseId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "PaymentRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIW8lWqrJjfmgCRfamYY5TcFLxLdYDR6r66605bPgdixbtb88ip66Ujq269VqjZ76w==");
        }
    }
}
