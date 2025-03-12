using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library_Management_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<float>(
                name: "AllowedBorrowHours",
                table: "BorrowRecords",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "BorrowRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_StudentId",
                table: "BorrowRecords",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowRecords_Students_StudentId",
                table: "BorrowRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_StudentId",
                table: "BorrowRecords");

            migrationBuilder.DropIndex(
                name: "IX_BorrowRecords_StudentId",
                table: "BorrowRecords");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "BorrowRecords");

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
