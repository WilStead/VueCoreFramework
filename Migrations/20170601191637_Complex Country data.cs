using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCCoreVue.Migrations
{
    public partial class ComplexCountrydata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Countries_Cities_CityId",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_CityId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Countries");

            migrationBuilder.AddColumn<Guid>(
                name: "CapitolId",
                table: "Countries",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaderId",
                table: "Countries",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Cities",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Leaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Age = table.Column<int>(nullable: false),
                    CreationTimestamp = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    UpdateTimestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CapitolId",
                table: "Countries",
                column: "CapitolId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_LeaderId",
                table: "Countries",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Cities_CapitolId",
                table: "Countries",
                column: "CapitolId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Leaders_LeaderId",
                table: "Countries",
                column: "LeaderId",
                principalTable: "Leaders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Countries_Cities_CapitolId",
                table: "Countries");

            migrationBuilder.DropForeignKey(
                name: "FK_Countries_Leaders_LeaderId",
                table: "Countries");

            migrationBuilder.DropTable(
                name: "Leaders");

            migrationBuilder.DropIndex(
                name: "IX_Countries_CapitolId",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_LeaderId",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CountryId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CapitolId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Cities");

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "Countries",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CityId",
                table: "Countries",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Countries_Cities_CityId",
                table: "Countries",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
