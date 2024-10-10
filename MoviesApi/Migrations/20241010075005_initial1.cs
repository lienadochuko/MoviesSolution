using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesApi.Migrations
{
    /// <inheritdoc />
    public partial class initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Films",
                columns: table => new
                {
                    FilmID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DirectorID = table.Column<int>(type: "int", nullable: true),
                    StudioID = table.Column<int>(type: "int", nullable: true),
                    Review = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryID = table.Column<int>(type: "int", nullable: true),
                    LanguageID = table.Column<int>(type: "int", nullable: true),
                    GenreID = table.Column<int>(type: "int", nullable: true),
                    RunTimeMinutes = table.Column<short>(type: "smallint", nullable: true),
                    CertificateID = table.Column<int>(type: "int", nullable: true),
                    BudgetDollars = table.Column<long>(type: "bigint", nullable: true),
                    BoxOfficeDollars = table.Column<long>(type: "bigint", nullable: true),
                    OscarNominations = table.Column<byte>(type: "tinyint", nullable: true),
                    OscarWins = table.Column<byte>(type: "tinyint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Films", x => x.FilmID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Films");
        }
    }
}
