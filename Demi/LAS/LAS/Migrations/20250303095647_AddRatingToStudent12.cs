using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToStudent12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "AllowedBorrowHours",
                table: "BorrowRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AllowedBorrowHours",
                table: "BorrowRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
