using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Converse.Migrations
{
    public partial class AddPublicKeyToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "users",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "userdeviceids",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "userdeviceids");
        }
    }
}
