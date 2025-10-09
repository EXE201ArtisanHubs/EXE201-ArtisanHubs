using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanHubs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "account",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "account",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "account");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "account");
        }
    }
}
