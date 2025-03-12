using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentandSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "BorrowRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "BorrowRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "School",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "BorrowRecords");

            migrationBuilder.DropColumn(
                name: "School",
                table: "BorrowRecords");
        }
    }
}
