using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class MoveRatingToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentRatings");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Students",
                type: "float",
                nullable: false,
                defaultValue: 5.0,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Students",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 5.0);

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
    }
}
