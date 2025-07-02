using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RigaMetro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithTimestampWithoutTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    IsClockwiseDirection = table.Column<bool>(type: "boolean", nullable: false),
                    StartWorkTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndWorkTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.LineID);
                });

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    StationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.StationID);
                });

            migrationBuilder.CreateTable(
                name: "LineSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LineID = table.Column<int>(type: "integer", nullable: false),
                    TripNumber = table.Column<int>(type: "integer", nullable: false),
                    IsClockwise = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_LineSchedules_Lines_LineID",
                        column: x => x.LineID,
                        principalTable: "Lines",
                        principalColumn: "LineID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trains",
                columns: table => new
                {
                    TrainID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LineID = table.Column<int>(type: "integer", nullable: false),
                    TrainName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trains", x => x.TrainID);
                    table.ForeignKey(
                        name: "FK_Trains_Lines_LineID",
                        column: x => x.LineID,
                        principalTable: "Lines",
                        principalColumn: "LineID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineStations",
                columns: table => new
                {
                    LineID = table.Column<int>(type: "integer", nullable: false),
                    StationID = table.Column<int>(type: "integer", nullable: false),
                    StationOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineStations", x => new { x.LineID, x.StationID });
                    table.ForeignKey(
                        name: "FK_LineStations_Lines_LineID",
                        column: x => x.LineID,
                        principalTable: "Lines",
                        principalColumn: "LineID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineStations_Stations_StationID",
                        column: x => x.StationID,
                        principalTable: "Stations",
                        principalColumn: "StationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeBetweenStations",
                columns: table => new
                {
                    FromStationID = table.Column<int>(type: "integer", nullable: false),
                    ToStationID = table.Column<int>(type: "integer", nullable: false),
                    TimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    DistanceM = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBetweenStations", x => new { x.FromStationID, x.ToStationID });
                    table.ForeignKey(
                        name: "FK_TimeBetweenStations_Stations_FromStationID",
                        column: x => x.FromStationID,
                        principalTable: "Stations",
                        principalColumn: "StationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeBetweenStations_Stations_ToStationID",
                        column: x => x.ToStationID,
                        principalTable: "Stations",
                        principalColumn: "StationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleStops",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "integer", nullable: false),
                    StationOrder = table.Column<int>(type: "integer", nullable: false),
                    StationID = table.Column<int>(type: "integer", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleStops", x => new { x.ScheduleID, x.StationOrder });
                    table.ForeignKey(
                        name: "FK_ScheduleStops_LineSchedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "LineSchedules",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleStops_Stations_StationID",
                        column: x => x.StationID,
                        principalTable: "Stations",
                        principalColumn: "StationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainAssignments",
                columns: table => new
                {
                    TrainID = table.Column<int>(type: "integer", nullable: false),
                    ScheduleID = table.Column<int>(type: "integer", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainAssignments", x => new { x.TrainID, x.ScheduleID, x.AssignmentDate });
                    table.ForeignKey(
                        name: "FK_TrainAssignments_LineSchedules_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "LineSchedules",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainAssignments_Trains_TrainID",
                        column: x => x.TrainID,
                        principalTable: "Trains",
                        principalColumn: "TrainID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Lines",
                columns: new[] { "LineID", "Color", "EndWorkTime", "IsClockwiseDirection", "Name", "StartWorkTime" },
                values: new object[] { 1, "#FF0000", new DateTime(2000, 1, 1, 23, 0, 0, 0, DateTimeKind.Unspecified), true, "Sarkandaugava–Ziepniekkalns", new DateTime(2000, 1, 1, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "StationID", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, 57.003383999999997, 24.118736999999999, "Sarkandaugava" },
                    { 2, 56.991002000000002, 24.122138, "Rupnica RER" },
                    { 3, 56.974578999999999, 24.111671000000001, "Ramulu iela" },
                    { 4, 56.964005999999998, 24.106183000000001, "Petersala" },
                    { 5, 56.956935000000001, 24.101257, "Kronvalda parks" },
                    { 6, 56.947561, 24.119751999999998, "Stacijas laukums" },
                    { 7, 56.933002999999999, 24.121713, "Zaķusala" },
                    { 8, 56.919696999999999, 24.098175999999999, "Straume" },
                    { 9, 56.912930000000003, 24.069526, "Dzintars" },
                    { 10, 56.898448000000002, 24.092072999999999, "Ziepniekkalns" }
                });

            migrationBuilder.InsertData(
                table: "LineStations",
                columns: new[] { "LineID", "StationID", "StationOrder" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 1, 2, 2 },
                    { 1, 3, 3 },
                    { 1, 4, 4 },
                    { 1, 5, 5 },
                    { 1, 6, 6 },
                    { 1, 7, 7 },
                    { 1, 8, 8 },
                    { 1, 9, 9 },
                    { 1, 10, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineSchedules_LineID",
                table: "LineSchedules",
                column: "LineID");

            migrationBuilder.CreateIndex(
                name: "IX_LineStations_StationID",
                table: "LineStations",
                column: "StationID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleStops_StationID",
                table: "ScheduleStops",
                column: "StationID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBetweenStations_ToStationID",
                table: "TimeBetweenStations",
                column: "ToStationID");

            migrationBuilder.CreateIndex(
                name: "IX_TrainAssignments_ScheduleID",
                table: "TrainAssignments",
                column: "ScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_LineID",
                table: "Trains",
                column: "LineID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineStations");

            migrationBuilder.DropTable(
                name: "ScheduleStops");

            migrationBuilder.DropTable(
                name: "TimeBetweenStations");

            migrationBuilder.DropTable(
                name: "TrainAssignments");

            migrationBuilder.DropTable(
                name: "Stations");

            migrationBuilder.DropTable(
                name: "LineSchedules");

            migrationBuilder.DropTable(
                name: "Trains");

            migrationBuilder.DropTable(
                name: "Lines");
        }
    }
}
