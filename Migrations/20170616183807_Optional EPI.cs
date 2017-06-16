using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCCoreVue.Migrations
{
    public partial class OptionalEPI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "EpiIndex",
                table: "Countries",
                nullable: true,
                oldClrType: typeof(double));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "EpiIndex",
                table: "Countries",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
