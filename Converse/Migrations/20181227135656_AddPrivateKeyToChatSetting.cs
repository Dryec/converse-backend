using Microsoft.EntityFrameworkCore.Migrations;

namespace Converse.Migrations
{
    public partial class AddPrivateKeyToChatSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "chatsettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "chatsettings");
        }
    }
}
