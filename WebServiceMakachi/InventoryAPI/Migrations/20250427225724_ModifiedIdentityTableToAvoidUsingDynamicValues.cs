using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedIdentityTableToAvoidUsingDynamicValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "PwdExpiryDate",
                table: "AspNetUsers",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 4, 27, 23, 42, 4, 396, DateTimeKind.Local).AddTicks(283));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "AspNetUsers",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 4, 27, 23, 42, 4, 388, DateTimeKind.Local).AddTicks(1538));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "PwdExpiryDate",
                table: "AspNetUsers",
                type: "datetime",
                nullable: true,
                defaultValue: new DateTime(2025, 4, 27, 23, 42, 4, 396, DateTimeKind.Local).AddTicks(283),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDate",
                table: "AspNetUsers",
                type: "datetime",
                nullable: true,
                defaultValue: new DateTime(2025, 4, 27, 23, 42, 4, 388, DateTimeKind.Local).AddTicks(1538),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);
        }
    }
}
