using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppsNoAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionID",
                table: "Users");
        }
    }
}
