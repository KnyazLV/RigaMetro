using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RigaMetro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainWorkTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndWorkTime",
                table: "Trains",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartWorkTime",
                table: "Trains",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndWorkTime",
                table: "Trains");

            migrationBuilder.DropColumn(
                name: "StartWorkTime",
                table: "Trains");
        }
    }
}
