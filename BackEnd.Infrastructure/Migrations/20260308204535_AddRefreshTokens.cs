using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InKindDonations_StaffMembers_RecordedById",
                table: "InKindDonations");

            migrationBuilder.DropIndex(
                name: "IX_InKindDonations_RecordedById",
                table: "InKindDonations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sponsorships");

            migrationBuilder.DropColumn(
                name: "RecordedById",
                table: "InKindDonations");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_RecordedByStaffId",
                table: "InKindDonations",
                column: "RecordedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_InKindDonations_StaffMembers_RecordedByStaffId",
                table: "InKindDonations",
                column: "RecordedByStaffId",
                principalTable: "StaffMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InKindDonations_StaffMembers_RecordedByStaffId",
                table: "InKindDonations");

            migrationBuilder.DropIndex(
                name: "IX_InKindDonations_RecordedByStaffId",
                table: "InKindDonations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sponsorships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecordedById",
                table: "InKindDonations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_RecordedById",
                table: "InKindDonations",
                column: "RecordedById");

            migrationBuilder.AddForeignKey(
                name: "FK_InKindDonations_StaffMembers_RecordedById",
                table: "InKindDonations",
                column: "RecordedById",
                principalTable: "StaffMembers",
                principalColumn: "Id");
        }
    }
}
