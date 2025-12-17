using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiV1ControlleurMonstre.Migrations
{
    /// <inheritdoc />
    public partial class caughtmonsters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaughtMonsters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    monstreCaughtId = table.Column<int>(type: "int", nullable: false),
                    whoHasCaughtId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaughtMonsters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaughtMonsters_Monstre_monstreCaughtId",
                        column: x => x.monstreCaughtId,
                        principalTable: "Monstre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaughtMonsters_Personnages_whoHasCaughtId",
                        column: x => x.whoHasCaughtId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CaughtMonsters_monstreCaughtId",
                table: "CaughtMonsters",
                column: "monstreCaughtId");

            migrationBuilder.CreateIndex(
                name: "IX_CaughtMonsters_whoHasCaughtId",
                table: "CaughtMonsters",
                column: "whoHasCaughtId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaughtMonsters");
        }
    }
}
