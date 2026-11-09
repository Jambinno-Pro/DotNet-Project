using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCompanySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternativePhone",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativePhone",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "AppSettings");
        }
    }
}
