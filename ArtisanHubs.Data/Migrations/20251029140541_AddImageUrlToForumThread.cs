using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanHubs.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToForumThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ForumThreads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ForumThreads");
        }
    }
}
