using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library_Management_Project.Migrations
{
    /// <inheritdoc />
    public partial class Addsomething : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_StudentId",
                table: "BorrowRecords");

            migrationBuilder.DropIndex(
                name: "IX_BorrowRecords_StudentId",
                table: "BorrowRecords");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "BorrowRecords");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Students",
                type: "float",
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double),
                oldType: "float");

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

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Books",
                type: "NVARCHAR(100)",
                nullable: false,
                collation: "SQL_Latin1_General_CP1_CS_AS",
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

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Students",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 5.0);

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "BorrowRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(100)",
                oldCollation: "SQL_Latin1_General_CP1_CS_AS");

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
    }
}
