using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiV1ControlleurMonstre.Migrations
{
    /// <inheritdoc />
    public partial class QuestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quetes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeQuete = table.Column<int>(type: "int", nullable: false),
                    DestinationPositionX = table.Column<int>(type: "int", nullable: true),
                    DestinationPositionY = table.Column<int>(type: "int", nullable: true),
                    TypeMonstre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreATuer = table.Column<int>(type: "int", nullable: false),
                    NombreActuellementTuer = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quetes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quetes_Tuiles_DestinationPositionX_DestinationPositionY",
                        columns: x => new { x.DestinationPositionX, x.DestinationPositionY },
                        principalTable: "Tuiles",
                        principalColumns: new[] { "PositionX", "PositionY" });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Quetes_DestinationPositionX_DestinationPositionY",
                table: "Quetes",
                columns: new[] { "DestinationPositionX", "DestinationPositionY" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quetes");
        }
    }
}
