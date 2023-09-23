using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IL.RulesBasedOutputCache.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    RuleAction = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    VaryByQueryString = table.Column<bool>(type: "bit", nullable: false),
                    ResponseExpirationTimeSpan = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachingRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachingRules");
        }
    }
}
