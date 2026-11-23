using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalProductPass.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantSkusAndCarbonIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DifferentiationCode",
                table: "Products",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndOfLifeGuidance",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gtin",
                table: "Products",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecyclabilityScore",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RepairabilityIndex",
                table: "Products",
                type: "decimal(3,1)",
                precision: 3,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WashInstructions",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterUsageLiters",
                table: "Products",
                type: "decimal(12,1)",
                precision: 12,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Products",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "ProductItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifferentiationCode",
                table: "ProductItems",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "ProductItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "ProductItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarbonIndexKgPerKg",
                table: "Materials",
                type: "decimal(7,3)",
                precision: 7,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateCode",
                table: "Materials",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifferentiationCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EndOfLifeGuidance",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Gtin",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RecyclabilityScore",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RepairabilityIndex",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WashInstructions",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WaterUsageLiters",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "ProductItems");

            migrationBuilder.DropColumn(
                name: "DifferentiationCode",
                table: "ProductItems");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "ProductItems");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "ProductItems");

            migrationBuilder.DropColumn(
                name: "CarbonIndexKgPerKg",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "CertificateCode",
                table: "Materials");
        }
    }
}
