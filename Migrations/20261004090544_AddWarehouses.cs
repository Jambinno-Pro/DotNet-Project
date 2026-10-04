using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Warehouses",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
            City = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Warehouses", x => x.Id);
        });

    migrationBuilder.AddColumn<int>(
        name: "WarehouseId",
        table: "Products",
        type: "int",
        nullable: true);

    migrationBuilder.InsertData(
        table: "Warehouses",
        columns: new[]
        {
            "Name",
            "Code",
            "Address",
            "City",
            "Country",
            "CreatedAt"
        },
        values: new object[,]
        {
            {
                "Cape Town Warehouse",
                "CPT",
                "Cape Town",
                "Cape Town",
                "South Africa",
                DateTime.UtcNow
            },
            {
                "Johannesburg Warehouse",
                "JHB",
                "Johannesburg",
                "Johannesburg",
                "South Africa",
                DateTime.UtcNow
            },
            {
                "Durban Warehouse",
                "DUR",
                "Durban",
                "Durban",
                "South Africa",
                DateTime.UtcNow
            },
            {
                "Harare Warehouse",
                "HRE",
                "Harare",
                "Harare",
                "Zimbabwe",
                DateTime.UtcNow
            }
        });

    migrationBuilder.Sql(@"
        UPDATE Products
        SET WarehouseId = Warehouses.Id
        FROM Products
        INNER JOIN Warehouses
            ON Warehouses.Code = 'CPT'
        WHERE Products.Warehouse = 'Cape Town';
    ");

    migrationBuilder.DropColumn(
        name: "Warehouse",
        table: "Products");

    migrationBuilder.CreateIndex(
        name: "IX_Products_WarehouseId",
        table: "Products",
        column: "WarehouseId");

    migrationBuilder.AddForeignKey(
        name: "FK_Products_Warehouses_WarehouseId",
        table: "Products",
        column: "WarehouseId",
        principalTable: "Warehouses",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Warehouses_WarehouseId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Products_WarehouseId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Warehouse",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
