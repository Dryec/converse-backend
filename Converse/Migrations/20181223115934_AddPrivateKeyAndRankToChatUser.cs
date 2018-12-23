using Microsoft.EntityFrameworkCore.Migrations;

namespace Converse.Migrations
{
    public partial class AddPrivateKeyAndRankToChatUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
	        migrationBuilder.DropColumn(
		        name: "IsAdmin",
		        table: "chatusers"
	        );

			migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "chatusers",
                nullable: true);
			
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "chatusers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "chatusers");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "chatusers");
        }
    }
}
