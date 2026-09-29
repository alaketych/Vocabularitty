using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vocabularity.Infrastructure.Database.Migrations;

public partial class RenameDictionaryEntriesToWords : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DictionaryEntries_Dictionaries_DictionaryId",
            table: "DictionaryEntries");
        migrationBuilder.DropPrimaryKey(name: "PK_DictionaryEntries", table: "DictionaryEntries");

        migrationBuilder.RenameTable(name: "DictionaryEntries", newName: "Words");
        migrationBuilder.RenameIndex(
            name: "IX_DictionaryEntries_DictionaryId_CreatedAt",
            table: "Words",
            newName: "IX_Words_DictionaryId_CreatedAt");

        migrationBuilder.AddPrimaryKey(name: "PK_Words", table: "Words", column: "Id");
        migrationBuilder.AddForeignKey(
            name: "FK_Words_Dictionaries_DictionaryId",
            table: "Words",
            column: "DictionaryId",
            principalTable: "Dictionaries",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Words_Dictionaries_DictionaryId", table: "Words");
        migrationBuilder.DropPrimaryKey(name: "PK_Words", table: "Words");

        migrationBuilder.RenameTable(name: "Words", newName: "DictionaryEntries");
        migrationBuilder.RenameIndex(
            name: "IX_Words_DictionaryId_CreatedAt",
            table: "DictionaryEntries",
            newName: "IX_DictionaryEntries_DictionaryId_CreatedAt");

        migrationBuilder.AddPrimaryKey(
            name: "PK_DictionaryEntries", table: "DictionaryEntries", column: "Id");
        migrationBuilder.AddForeignKey(
            name: "FK_DictionaryEntries_Dictionaries_DictionaryId",
            table: "DictionaryEntries",
            column: "DictionaryId",
            principalTable: "Dictionaries",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
