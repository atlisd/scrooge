using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scrooge.Api.Migrations
{
    /// <inheritdoc />
    public partial class SessionLastUsedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "AppSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "AppSessions");
        }
    }
}
