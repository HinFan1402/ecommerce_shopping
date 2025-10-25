using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerce_shopping.Migrations
{
    public partial class FKProductQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantities_ProductId",
                table: "ProductQuantities",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantities_Products_ProductId",
                table: "ProductQuantities",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantities_Products_ProductId",
                table: "ProductQuantities");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantities_ProductId",
                table: "ProductQuantities");
        }
    }
}
