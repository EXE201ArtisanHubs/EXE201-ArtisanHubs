using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanHubs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCodeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "weight",
                table: "product",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<long>(
                name: "order_code",
                table: "Order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_fee",
                table: "Order",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "weight",
                table: "product");

            migrationBuilder.DropColumn(
                name: "order_code",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "shipping_fee",
                table: "Order");
        }
    }
}
