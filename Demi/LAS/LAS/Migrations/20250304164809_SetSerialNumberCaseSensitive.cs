using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class SetSerialNumberCaseSensitive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Books",
                type: "NVARCHAR(100)",
                nullable: false,
                collation: "SQL_Latin1_General_CP1_CS_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(100)",
                oldCollation: "SQL_Latin1_General_CP1_CS_AS");
        }
    }
}
