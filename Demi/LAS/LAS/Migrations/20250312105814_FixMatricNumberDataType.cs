using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class FixMatricNumberDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Drop foreign key first
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords");

            // 2️⃣ Change MatricNumber column type in Students
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "NVARCHAR(20)", // Adjust size if needed
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // 3️⃣ Change MatricNumber column type in BorrowRecords
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "NVARCHAR(20)", // Ensure it matches Students table
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // 4️⃣ Recreate foreign key
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

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(20)");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords",
                column: "MatricNumber",
                principalTable: "Students",
                principalColumn: "MatricNumber",
                onDelete: ReferentialAction.Cascade);
        }

    }
}
