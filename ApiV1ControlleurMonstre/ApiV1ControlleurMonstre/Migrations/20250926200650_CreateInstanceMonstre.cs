using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiV1ControlleurMonstre.Migrations
{
    /// <inheritdoc />
    public partial class CreateInstanceMonstre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "utilisateur",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InstanceMonstres",
                columns: table => new
                {
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    MonstreId = table.Column<int>(type: "int", nullable: false),
                    Niveau = table.Column<int>(type: "int", nullable: false),
                    PointsVieMax = table.Column<int>(type: "int", nullable: false),
                    PointsVieActuels = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceMonstres", x => new { x.PositionX, x.PositionY });
                    table.ForeignKey(
                        name: "FK_InstanceMonstres_Monstre_MonstreId",
                        column: x => x.MonstreId,
                        principalTable: "Monstre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceMonstres_MonstreId",
                table: "InstanceMonstres",
                column: "MonstreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstanceMonstres");

            migrationBuilder.UpdateData(
                table: "utilisateur",
                keyColumn: "Token",
                keyValue: null,
                column: "Token",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "utilisateur",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
