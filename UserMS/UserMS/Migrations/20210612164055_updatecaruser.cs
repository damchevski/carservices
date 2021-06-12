using Microsoft.EntityFrameworkCore.Migrations;

namespace UserMS.Migrations
{
    public partial class updatecaruser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCar");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCar",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCar", x => new { x.UserId, x.CarId });
                });
        }
    }
}
