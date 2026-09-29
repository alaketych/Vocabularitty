using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vocabularity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaryPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Dictionaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Preserve the previous CreatedAt/Id listing order independently for each owner.
            migrationBuilder.Sql("""
                WITH OrderedDictionaries AS
                (
                    SELECT [Id],
                           ROW_NUMBER() OVER (
                               PARTITION BY [UserId] ORDER BY [CreatedAt], [Id]) - 1 AS [NewPosition]
                    FROM [Dictionaries]
                )
                UPDATE dictionary
                SET [Position] = ordered.[NewPosition]
                FROM [Dictionaries] AS dictionary
                INNER JOIN OrderedDictionaries AS ordered ON dictionary.[Id] = ordered.[Id];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Dictionaries_UserId_Position",
                table: "Dictionaries",
                columns: new[] { "UserId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dictionaries_UserId_Position",
                table: "Dictionaries");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Dictionaries");
        }
    }
}
