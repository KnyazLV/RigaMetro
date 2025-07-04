using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RigaMetro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    LineID = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
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
                    StationID = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
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
                    ScheduleID = table.Column<string>(type: "text", nullable: false),
                    LineID = table.Column<string>(type: "character varying(8)", nullable: false),
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
                    TrainID = table.Column<string>(type: "text", nullable: false),
                    LineID = table.Column<string>(type: "character varying(8)", nullable: false),
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
                    LineID = table.Column<string>(type: "character varying(8)", nullable: false),
                    StationID = table.Column<string>(type: "character varying(8)", nullable: false),
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
                    FromStationID = table.Column<string>(type: "character varying(8)", nullable: false),
                    ToStationID = table.Column<string>(type: "character varying(8)", nullable: false),
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
                    ScheduleID = table.Column<string>(type: "text", nullable: false),
                    StationOrder = table.Column<int>(type: "integer", nullable: false),
                    StationID = table.Column<string>(type: "character varying(8)", nullable: false),
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
                    TrainID = table.Column<string>(type: "text", nullable: false),
                    ScheduleID = table.Column<string>(type: "text", nullable: false),
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
                values: new object[] { "LN01", "#FF0000", new DateTime(2000, 1, 1, 23, 0, 0, 0, DateTimeKind.Unspecified), true, "Sarkandaugava–Ziepniekkalns", new DateTime(2000, 1, 1, 6, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "StationID", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { "ST101", 57.003383999999997, 24.118736999999999, "Sarkandaugava" },
                    { "ST102", 56.991002000000002, 24.122138, "Rupnica RER" },
                    { "ST103", 56.974578999999999, 24.111671000000001, "Ramulu iela" },
                    { "ST104", 56.964005999999998, 24.106183000000001, "Petersala" },
                    { "ST105", 56.956935000000001, 24.101257, "Kronvalda parks" },
                    { "ST106", 56.947561, 24.119751999999998, "Stacijas laukums" },
                    { "ST107", 56.933002999999999, 24.121713, "Zaķusala" },
                    { "ST108", 56.919696999999999, 24.098175999999999, "Straume" },
                    { "ST109", 56.912930000000003, 24.069526, "Dzintars" },
                    { "ST110", 56.898448000000002, 24.092072999999999, "Ziepniekkalns" }
                });

            migrationBuilder.InsertData(
                table: "LineStations",
                columns: new[] { "LineID", "StationID", "StationOrder" },
                values: new object[,]
                {
                    { "LN01", "ST101", 1 },
                    { "LN01", "ST102", 2 },
                    { "LN01", "ST103", 3 },
                    { "LN01", "ST104", 4 },
                    { "LN01", "ST105", 5 },
                    { "LN01", "ST106", 6 },
                    { "LN01", "ST107", 7 },
                    { "LN01", "ST108", 8 },
                    { "LN01", "ST109", 9 },
                    { "LN01", "ST110", 10 }
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
