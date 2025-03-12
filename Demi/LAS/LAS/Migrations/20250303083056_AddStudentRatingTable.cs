using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentRatingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowedBooks");

            migrationBuilder.CreateTable(
                name: "StudentRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatricNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false, defaultValue: 5.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRatings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentRatings");

            migrationBuilder.CreateTable(
                name: "BorrowedBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowedBorrowHours = table.Column<int>(type: "int", nullable: false),
                    BorrowTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MatricNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowedBooks", x => x.Id);
                });
        }
    }
}
