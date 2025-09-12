using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiV1ControlleurMonstre.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPersonnage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Utilisateurs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Utilisateurs");
        }
    }
}
