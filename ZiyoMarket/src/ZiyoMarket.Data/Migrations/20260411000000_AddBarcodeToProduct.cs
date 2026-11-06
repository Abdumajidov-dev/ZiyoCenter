using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiyoMarket.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Production bazada ustun qo'lda qo'shilgan bo'lishi mumkin, shuning uchun IF NOT EXISTS
            migrationBuilder.Sql(
                "ALTER TABLE \"Products\" ADD COLUMN IF NOT EXISTS \"Barcode\" character varying(100);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");
        }
    }
}
