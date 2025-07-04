using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RigaMetro.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainWorkTimeAndSeedTrains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Trains",
                columns: new[] { "TrainID", "EndWorkTime", "IsActive", "LineID", "StartWorkTime", "TrainName" },
                values: new object[,]
                {
                    { "TR001", new TimeSpan(0, 20, 0, 0, 0), true, "LN01", new TimeSpan(0, 8, 0, 0, 0), "TR–1" },
                    { "TR002", new TimeSpan(0, 19, 30, 0, 0), true, "LN01", new TimeSpan(0, 7, 30, 0, 0), "TR–2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Trains",
                keyColumn: "TrainID",
                keyValue: "TR001");

            migrationBuilder.DeleteData(
                table: "Trains",
                keyColumn: "TrainID",
                keyValue: "TR002");
        }
    }
}
