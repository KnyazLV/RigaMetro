using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RigaMetro.Infrastructure.Migrations
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
                    StartWorkTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndWorkTime = table.Column<TimeSpan>(type: "interval", nullable: false),
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
                values: new object[,]
                {
                    { "LN01", "#FF0000", new DateTime(2000, 1, 1, 23, 0, 0, 0, DateTimeKind.Unspecified), true, "Sarkandaugava–Ziepniekkalns", new DateTime(2000, 1, 1, 6, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "LN02", "#00B050", new DateTime(2000, 1, 1, 23, 0, 0, 0, DateTimeKind.Unspecified), true, "Imanta–Jugla", new DateTime(2000, 1, 1, 6, 0, 0, 0, DateTimeKind.Unspecified) },
                    { "LN03", "#0000FF", new DateTime(2000, 1, 1, 23, 0, 0, 0, DateTimeKind.Unspecified), true, "Dreilini–Buļļu kāpa", new DateTime(2000, 1, 1, 6, 0, 0, 0, DateTimeKind.Unspecified) }
                });

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
                    { "ST110", 56.898448000000002, 24.092072999999999, "Ziepniekkalns" },
                    { "ST201", 56.960270999999999, 24.014749999999999, "Imanta" },
                    { "ST202", 56.945292999999999, 24.002022, "Zolitūde" },
                    { "ST203", 56.930816, 24.036110000000001, "Pleskodāle" },
                    { "ST204", 56.946688999999999, 24.048093999999999, "Zasulauks" },
                    { "ST205", 56.935529000000002, 24.072192000000001, "Āgenskalns" },
                    { "ST206", 56.937130000000003, 24.086746999999999, "Uzvaras parks" },
                    { "ST208", 56.954588000000001, 24.118321999999999, "Esplanāde" },
                    { "ST209", 56.959961999999997, 24.130016999999999, "Vidzemes tirgus" },
                    { "ST210", 56.972436000000002, 24.141017999999999, "Brasa" },
                    { "ST211", 56.973675999999998, 24.166967, "VEF" },
                    { "ST212", 56.983054000000003, 24.200993, "Teika" },
                    { "ST213", 56.987684000000002, 24.229665000000001, "Alfa" },
                    { "ST214", 56.979464, 24.253133999999999, "Jugla" },
                    { "ST301", 56.943384999999999, 24.254300000000001, "Dreilini" },
                    { "ST302", 56.938481000000003, 24.210546000000001, "Plavnieki" },
                    { "ST303", 56.957673999999997, 24.184348, "Purvciems" },
                    { "ST304", 56.953285999999999, 24.156230000000001, "Daugavas stadions" },
                    { "ST305", 56.955894999999998, 24.135721, "Matīsa Iela" },
                    { "ST306", 56.949916000000002, 24.087104, "Ķīpsala" },
                    { "ST307", 56.966904, 24.058005999999999, "Iļģuciems" },
                    { "ST308", 56.975793000000003, 24.032716000000001, "Lačupe" },
                    { "ST309", 56.984631999999998, 24.026593999999999, "Kleisti" },
                    { "ST310", 57.001215999999999, 23.987065000000001, "Buļļu kāpa" }
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
                    { "LN01", "ST110", 10 },
                    { "LN02", "ST106", 7 },
                    { "LN02", "ST201", 1 },
                    { "LN02", "ST202", 2 },
                    { "LN02", "ST203", 3 },
                    { "LN02", "ST204", 4 },
                    { "LN02", "ST205", 5 },
                    { "LN02", "ST206", 6 },
                    { "LN02", "ST208", 8 },
                    { "LN02", "ST209", 9 },
                    { "LN02", "ST210", 10 },
                    { "LN02", "ST211", 11 },
                    { "LN02", "ST212", 12 },
                    { "LN02", "ST213", 13 },
                    { "LN02", "ST214", 14 },
                    { "LN03", "ST105", 7 },
                    { "LN03", "ST208", 6 },
                    { "LN03", "ST301", 1 },
                    { "LN03", "ST302", 2 },
                    { "LN03", "ST303", 3 },
                    { "LN03", "ST304", 4 },
                    { "LN03", "ST305", 5 },
                    { "LN03", "ST306", 8 },
                    { "LN03", "ST307", 9 },
                    { "LN03", "ST308", 10 },
                    { "LN03", "ST309", 11 },
                    { "LN03", "ST310", 12 }
                });

            migrationBuilder.InsertData(
                table: "Trains",
                columns: new[] { "TrainID", "EndWorkTime", "IsActive", "LineID", "StartWorkTime", "TrainName" },
                values: new object[,]
                {
                    { "TR001", new TimeSpan(0, 20, 0, 0, 0), true, "LN01", new TimeSpan(0, 8, 0, 0, 0), "TR–1" },
                    { "TR002", new TimeSpan(0, 19, 30, 0, 0), true, "LN01", new TimeSpan(0, 7, 30, 0, 0), "TR–2" },
                    { "TR201", new TimeSpan(0, 21, 0, 0, 0), true, "LN02", new TimeSpan(0, 7, 0, 0, 0), "TR–Green–1" },
                    { "TR202", new TimeSpan(0, 21, 30, 0, 0), true, "LN02", new TimeSpan(0, 7, 30, 0, 0), "TR–Green–2" },
                    { "TR301", new TimeSpan(0, 22, 0, 0, 0), true, "LN03", new TimeSpan(0, 6, 0, 0, 0), "TR–Blue–1" },
                    { "TR302", new TimeSpan(0, 22, 30, 0, 0), true, "LN03", new TimeSpan(0, 6, 30, 0, 0), "TR–Blue–2" }
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
