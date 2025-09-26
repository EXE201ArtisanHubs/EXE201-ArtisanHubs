using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanHubs.Data.Migrations
{
    public partial class RemovePasswordColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password",   // 👈 cột bạn muốn xóa
                table: "account");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",   // 👈 thêm lại nếu rollback
                table: "Account",
                type: "text",       // hoặc varchar(255) tùy schema cũ
                nullable: true
            );
        }
    }
}
