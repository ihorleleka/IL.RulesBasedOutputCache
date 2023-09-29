using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IL.RulesBasedOutputCache.Migrations
{
    /// <inheritdoc />
    public partial class VaryByAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VaryByCulture",
                table: "CachingRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VaryByHost",
                table: "CachingRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VaryByUser",
                table: "CachingRules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaryByCulture",
                table: "CachingRules");

            migrationBuilder.DropColumn(
                name: "VaryByHost",
                table: "CachingRules");

            migrationBuilder.DropColumn(
                name: "VaryByUser",
                table: "CachingRules");
        }
    }
}
