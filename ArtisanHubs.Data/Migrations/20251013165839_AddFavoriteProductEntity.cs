using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanHubs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteProductEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteProducts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteProducts", x => new { x.AccountId, x.ProductId });
                    table.ForeignKey(
                        name: "favoriteproduct_account_id_fkey",
                        column: x => x.AccountId,
                        principalTable: "account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "favoriteproduct_product_id_fkey",
                        column: x => x.ProductId,
                        principalTable: "product",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteProducts_ProductId",
                table: "FavoriteProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteProducts");
        }
    }
}
