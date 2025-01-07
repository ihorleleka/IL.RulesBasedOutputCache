using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IL.RulesBasedOutputCache.Migrations
{
    /// <inheritdoc />
    public partial class TimeSpanToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ResponseExpirationTimeSpan",
                table: "CachingRules",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ResponseExpirationTimeSpan",
                table: "CachingRules",
                type: "time",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
