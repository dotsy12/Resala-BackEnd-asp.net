using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEA8KZ44DyNRaxDW/bN406WcHVgE+43AwJMhb0suITJSUV1SIttVPHzSiV3y0bL6QWQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDZffCZ1Jv0MApj6ocE4KMf3SPhwXC54xd93VsfFUTGo7wUq9IuNZL8SrGw7iMxqIg==");
        }
    }
}
