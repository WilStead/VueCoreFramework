using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCCoreVue.Migrations
{
    public partial class detailedcityleadermodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "Leaders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Transit",
                table: "Cities",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Cities",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Leaders");

            migrationBuilder.DropColumn(
                name: "Transit",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Cities");
        }
    }
}
