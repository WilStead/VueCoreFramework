using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCCoreVue.Migrations
{
    public partial class Leadertimespanfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeInOffice",
                table: "Leaders");

            migrationBuilder.AddColumn<long>(
                name: "TimeInOfficeTicks",
                table: "Leaders",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeInOfficeTicks",
                table: "Leaders");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeInOffice",
                table: "Leaders",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
