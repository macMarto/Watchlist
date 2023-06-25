using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchlist.Migrations
{
    public partial class GanreNameAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GanreName",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GanreName",
                table: "Movies");
        }
    }
}
