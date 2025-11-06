using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiV1ControlleurMonstre.Migrations
{
    /// <inheritdoc />
    public partial class QuestMigrationPesonnage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonnageId",
                table: "Quetes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quetes_PersonnageId",
                table: "Quetes",
                column: "PersonnageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quetes_Personnages_PersonnageId",
                table: "Quetes",
                column: "PersonnageId",
                principalTable: "Personnages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quetes_Personnages_PersonnageId",
                table: "Quetes");

            migrationBuilder.DropIndex(
                name: "IX_Quetes_PersonnageId",
                table: "Quetes");

            migrationBuilder.DropColumn(
                name: "PersonnageId",
                table: "Quetes");
        }
    }
}
