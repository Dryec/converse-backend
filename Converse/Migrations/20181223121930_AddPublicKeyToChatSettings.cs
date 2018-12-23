using Microsoft.EntityFrameworkCore.Migrations;

namespace Converse.Migrations
{
    public partial class AddPublicKeyToChatSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "chatsettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "chatsettings");
        }
    }
}
