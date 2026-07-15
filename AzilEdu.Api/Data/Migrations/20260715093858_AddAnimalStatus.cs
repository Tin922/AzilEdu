using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AzilEdu.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AnimalStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Dostupna za udomljenje" },
                    { 2, "Rezervirana" },
                    { 3, "Udomljena" },
                    { 4, "Na liječenju" }
                });

            migrationBuilder.AddColumn<int>(
                name: "AnimalStatusId",
                table: "Animals",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                """
                UPDATE Animals
                SET AnimalStatusId = CASE WHEN IsAdopted = 1 THEN 3 ELSE 1 END;
                """);

            migrationBuilder.DropColumn(
                name: "IsAdopted",
                table: "Animals");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_AnimalStatusId",
                table: "Animals",
                column: "AnimalStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_AnimalStatuses_AnimalStatusId",
                table: "Animals",
                column: "AnimalStatusId",
                principalTable: "AnimalStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_AnimalStatuses_AnimalStatusId",
                table: "Animals");

            migrationBuilder.DropTable(
                name: "AnimalStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Animals_AnimalStatusId",
                table: "Animals");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdopted",
                table: "Animals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE Animals
                SET IsAdopted = CASE WHEN AnimalStatusId = 3 THEN 1 ELSE 0 END;
                """);

            migrationBuilder.DropColumn(
                name: "AnimalStatusId",
                table: "Animals");
        }
    }
}
