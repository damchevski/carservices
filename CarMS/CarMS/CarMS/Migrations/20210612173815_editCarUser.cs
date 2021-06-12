using Microsoft.EntityFrameworkCore.Migrations;

namespace CarMS.Migrations
{
    public partial class editCarUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCars",
                table: "UserCars");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserCars");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserCars",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCars",
                table: "UserCars",
                columns: new[] { "Username", "CarId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCars",
                table: "UserCars");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserCars");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserCars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCars",
                table: "UserCars",
                columns: new[] { "UserId", "CarId" });
        }
    }
}
