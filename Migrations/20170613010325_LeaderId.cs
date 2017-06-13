using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCCoreVue.Migrations
{
    public partial class LeaderId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaders_Countries_LeaderCountryId",
                table: "Leaders");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaders_Countries_LeaderCountryId",
                table: "Leaders",
                column: "LeaderCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaders_Countries_LeaderCountryId",
                table: "Leaders");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaders_Countries_LeaderCountryId",
                table: "Leaders",
                column: "LeaderCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
