using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmergencyCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrgencyLevel = table.Column<int>(type: "int", nullable: false),
                    RequiredAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiredCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CollectedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CollectedCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyCases", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENsyWjE/iWrBWRxUgE75twiJv8D0AxWZKlYGeMl6W/GGETJvno2tbjGuvpgby11D0g==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyCases");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK/QfU3XoK9vxtbtCbL9TkeOx+jvKfNmuzablZY2KbsPNfbh2tNgkPlhqt9LD4JirA==");
        }
    }
}
