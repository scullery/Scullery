using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scullery.EntityFrameworkCore.Migrations;

public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Jobs",
            columns: table => new
            {
                Id = table.Column<long>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.SerialColumn)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: true),
                Scheduled = table.Column<DateTime>(nullable: true),
                TimeZone = table.Column<string>(nullable: true),
                Status = table.Column<int>(nullable: false),
                Type = table.Column<string>(nullable: true),
                Method = table.Column<string>(nullable: true),
                Returns = table.Column<string>(nullable: true),
                IsStatic = table.Column<bool>(nullable: false),
                Parameters = table.Column<string>(nullable: true),
                Arguments = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Jobs", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Jobs");
    }
}
