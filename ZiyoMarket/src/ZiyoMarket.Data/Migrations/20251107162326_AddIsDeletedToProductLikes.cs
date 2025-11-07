using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiyoMarket.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToProductLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductLikes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductLikes");
        }
    }
}
