using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cover.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToAppCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "AppCredentials",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ISK");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyDecimals",
                table: "AppCredentials",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "AppCredentials");

            migrationBuilder.DropColumn(
                name: "CurrencyDecimals",
                table: "AppCredentials");
        }
    }
}
