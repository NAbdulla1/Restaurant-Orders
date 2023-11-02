using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant_Orders.Migrations
{
    /// <inheritdoc />
    public partial class SetMenuItemIdToNullInOrderItemOnDeleteOfMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_menu_items_menu_item_id",
                table: "order_items");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_menu_items_menu_item_id",
                table: "order_items",
                column: "menu_item_id",
                principalTable: "menu_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_menu_items_menu_item_id",
                table: "order_items");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_menu_items_menu_item_id",
                table: "order_items",
                column: "menu_item_id",
                principalTable: "menu_items",
                principalColumn: "id");
        }
    }
}
