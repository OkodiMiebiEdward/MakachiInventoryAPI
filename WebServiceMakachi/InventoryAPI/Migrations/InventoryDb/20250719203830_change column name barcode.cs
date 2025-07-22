using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations.InventoryDb
{
    /// <inheritdoc />
    public partial class changecolumnnamebarcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Barcode",
                table: "Sales",
                newName: "Barcodenumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Barcodenumber",
                table: "Sales",
                newName: "Barcode");
        }
    }
}
