using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campanyes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PressupostTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    PressupostConsumit = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataInici = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DataFi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Estat = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campanyes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campanyes");
        }
    }
}
