using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppsNoAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddedCompanyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "password");
        }
    }
}
