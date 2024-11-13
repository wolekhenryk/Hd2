using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hd2.API.Migrations
{
    /// <inheritdoc />
    public partial class key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adresy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HouseNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypyOrganow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypyOrganow", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Osoby",
                columns: table => new
                {
                    Pesel = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Osoby", x => x.Pesel);
                    table.ForeignKey(
                        name: "FK_Osoby_Adresy_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Adresy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Szpitale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Szpitale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Szpitale_Adresy_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Adresy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Dawcy",
                columns: table => new
                {
                    Pesel = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    HealthStatus = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AliveDuringExtraction = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dawcy", x => x.Pesel);
                    table.ForeignKey(
                        name: "FK_Dawcy_Osoby_Pesel",
                        column: x => x.Pesel,
                        principalTable: "Osoby",
                        principalColumn: "Pesel",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pacjenci",
                columns: table => new
                {
                    Pesel = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    HealthStatus = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacjenci", x => x.Pesel);
                    table.ForeignKey(
                        name: "FK_Pacjenci_Osoby_Pesel",
                        column: x => x.Pesel,
                        principalTable: "Osoby",
                        principalColumn: "Pesel",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lekarze",
                columns: table => new
                {
                    Pesel = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Pwz = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HospitalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lekarze", x => x.Pesel);
                    table.ForeignKey(
                        name: "FK_Lekarze_Osoby_Pesel",
                        column: x => x.Pesel,
                        principalTable: "Osoby",
                        principalColumn: "Pesel",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lekarze_Szpitale_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Szpitale",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Narzady",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HarvestDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StorageType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrganTypeId = table.Column<int>(type: "int", nullable: false),
                    DonorPesel = table.Column<string>(type: "nvarchar(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Narzady", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Narzady_Dawcy_DonorPesel",
                        column: x => x.DonorPesel,
                        principalTable: "Dawcy",
                        principalColumn: "Pesel");
                    table.ForeignKey(
                        name: "FK_Narzady_TypyOrganow_OrganTypeId",
                        column: x => x.OrganTypeId,
                        principalTable: "TypyOrganow",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Zabiegi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientPesel = table.Column<string>(type: "nvarchar(11)", nullable: false),
                    HospitalId = table.Column<int>(type: "int", nullable: false),
                    OrganId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zabiegi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zabiegi_Narzady_OrganId",
                        column: x => x.OrganId,
                        principalTable: "Narzady",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Zabiegi_Pacjenci_PatientPesel",
                        column: x => x.PatientPesel,
                        principalTable: "Pacjenci",
                        principalColumn: "Pesel");
                    table.ForeignKey(
                        name: "FK_Zabiegi_Szpitale_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Szpitale",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DoctorProcedure",
                columns: table => new
                {
                    DoctorsPesel = table.Column<string>(type: "nvarchar(11)", nullable: false),
                    ProceduresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProcedure", x => new { x.DoctorsPesel, x.ProceduresId });
                    table.ForeignKey(
                        name: "FK_DoctorProcedure_Lekarze_DoctorsPesel",
                        column: x => x.DoctorsPesel,
                        principalTable: "Lekarze",
                        principalColumn: "Pesel",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorProcedure_Zabiegi_ProceduresId",
                        column: x => x.ProceduresId,
                        principalTable: "Zabiegi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Komplikacje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProcedureId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Komplikacje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Komplikacje_Zabiegi_ProcedureId",
                        column: x => x.ProcedureId,
                        principalTable: "Zabiegi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProcedure_ProceduresId",
                table: "DoctorProcedure",
                column: "ProceduresId");

            migrationBuilder.CreateIndex(
                name: "IX_Komplikacje_ProcedureId",
                table: "Komplikacje",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_HospitalId",
                table: "Lekarze",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Lekarze_Pwz",
                table: "Lekarze",
                column: "Pwz",
                unique: true,
                filter: "[Pwz] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Narzady_DonorPesel",
                table: "Narzady",
                column: "DonorPesel");

            migrationBuilder.CreateIndex(
                name: "IX_Narzady_OrganTypeId",
                table: "Narzady",
                column: "OrganTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Osoby_AddressId",
                table: "Osoby",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Szpitale_AddressId",
                table: "Szpitale",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zabiegi_HospitalId",
                table: "Zabiegi",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Zabiegi_OrganId",
                table: "Zabiegi",
                column: "OrganId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zabiegi_PatientPesel",
                table: "Zabiegi",
                column: "PatientPesel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorProcedure");

            migrationBuilder.DropTable(
                name: "Komplikacje");

            migrationBuilder.DropTable(
                name: "Lekarze");

            migrationBuilder.DropTable(
                name: "Zabiegi");

            migrationBuilder.DropTable(
                name: "Narzady");

            migrationBuilder.DropTable(
                name: "Pacjenci");

            migrationBuilder.DropTable(
                name: "Szpitale");

            migrationBuilder.DropTable(
                name: "Dawcy");

            migrationBuilder.DropTable(
                name: "TypyOrganow");

            migrationBuilder.DropTable(
                name: "Osoby");

            migrationBuilder.DropTable(
                name: "Adresy");
        }
    }
}
