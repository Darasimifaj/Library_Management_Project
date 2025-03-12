using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class addborrowedbooks12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowedBooks",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students",
                column: "MatricNumber");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_MatricNumber",
                table: "BorrowRecords",
                column: "MatricNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords",
                column: "MatricNumber",
                principalTable: "Students",
                principalColumn: "MatricNumber",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_BorrowRecords_MatricNumber",
                table: "BorrowRecords");

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "BorrowedBooks",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
