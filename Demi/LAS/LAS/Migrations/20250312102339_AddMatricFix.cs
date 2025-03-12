using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class AddMatricFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint in BorrowRecords
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords");

            // Drop the unique constraint on MatricNumber
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students");

            // Alter the MatricNumber column type in Students table
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Alter the MatricNumber column type in BorrowRecords table
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Re-add the unique constraint on MatricNumber in Students table
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students",
                column: "MatricNumber");

            // Re-add the foreign key constraint in BorrowRecords
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
            // Drop the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowRecords_Students_MatricNumber",
                table: "BorrowRecords");

            // Drop the unique constraint
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students");

            // Revert MatricNumber column type in Students
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            // Revert MatricNumber column type in BorrowRecords
            migrationBuilder.AlterColumn<string>(
                name: "MatricNumber",
                table: "BorrowRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            // Re-add the unique constraint
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Students_MatricNumber",
                table: "Students",
                column: "MatricNumber");

            // Re-add the foreign key constraint
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
