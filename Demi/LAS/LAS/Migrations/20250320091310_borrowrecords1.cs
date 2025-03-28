using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAS.Migrations
{
    /// <inheritdoc />
    public partial class borrowrecords1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Books",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        SerialNumber = table.Column<string>(type: "NVARCHAR(100)", nullable: false, collation: "SQL_Latin1_General_CP1_CS_AS"),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Year = table.Column<int>(type: "int", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Books", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "LateReturns",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        MatricNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LateReturnCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LateReturns", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PendingBorrows",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        MatricNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BorrowCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsApproved = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PendingBorrows", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PendingReturns",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        MatricNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ReturnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsApproved = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PendingReturns", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Students",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        MatricNumber = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        School = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Rating = table.Column<double>(type: "float", nullable: false, defaultValue: 5.0)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Students", x => x.Id);
            //        table.UniqueConstraint("AK_Students_MatricNumber", x => x.MatricNumber);
            //    });

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatricNumber = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BorrowTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllowedBorrowHours = table.Column<float>(type: "real", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false),
                    Overdue = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Students_MatricNumber",
                        column: x => x.MatricNumber,
                        principalTable: "Students",
                        principalColumn: "MatricNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_MatricNumber",
                table: "BorrowRecords",
                column: "MatricNumber");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Students_Email",
            //    table: "Students",
            //    column: "Email",
            //    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "LateReturns");

            migrationBuilder.DropTable(
                name: "PendingBorrows");

            migrationBuilder.DropTable(
                name: "PendingReturns");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
