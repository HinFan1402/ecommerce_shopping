using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerce_shopping.Migrations
{
    public partial class passwordNoHashForUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassWordNoHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassWordNoHash",
                table: "AspNetUsers");
        }
    }
}
