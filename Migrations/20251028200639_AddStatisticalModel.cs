using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerce_shopping.Migrations
{
    public partial class AddStatisticalModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Statisticals",
                table: "Statisticals");

            migrationBuilder.RenameTable(
                name: "Statisticals",
                newName: "Statistics");

            migrationBuilder.RenameColumn(
                name: "Sold",
                table: "Statistics",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "Revenue",
                table: "Statistics",
                newName: "OrderStatus");

            migrationBuilder.RenameColumn(
                name: "Profit",
                table: "Statistics",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Statistics",
                newName: "OrderDate");

            migrationBuilder.AddColumn<decimal>(
                name: "InPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Statistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Statistics",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Statistics",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "Statistics",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Statistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statistics",
                table: "Statistics",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_ProductId",
                table: "Statistics",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statistics_Products_ProductId",
                table: "Statistics",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statistics_Products_ProductId",
                table: "Statistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Statistics",
                table: "Statistics");

            migrationBuilder.DropIndex(
                name: "IX_Statistics_ProductId",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "InPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Statistics");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Statistics");

            migrationBuilder.RenameTable(
                name: "Statistics",
                newName: "Statisticals");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Statisticals",
                newName: "Sold");

            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "Statisticals",
                newName: "Revenue");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "Statisticals",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Statisticals",
                newName: "Profit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statisticals",
                table: "Statisticals",
                column: "Id");
        }
    }
}
