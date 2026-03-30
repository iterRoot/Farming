using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class t0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breeding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    ExpectedCalvingDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: true),
                    ActualCalvingDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeding", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BreedingRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<int>(type: "integer", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceByStaffId = table.Column<int>(type: "integer", nullable: true),
                    ServiceType = table.Column<string>(type: "text", nullable: true),
                    SireTag = table.Column<string>(type: "text", nullable: true),
                    PregnancyConfirmed = table.Column<bool>(type: "boolean", nullable: true),
                    PregnancyCheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalvingRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<int>(type: "integer", nullable: false),
                    CalvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumberOfCalves = table.Column<int>(type: "integer", nullable: false),
                    CalfTagNumbers = table.Column<string>(type: "text", nullable: true),
                    DeliveryType = table.Column<string>(type: "text", nullable: true),
                    Outcome = table.Column<string>(type: "text", nullable: true),
                    BirthWeightKg = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalvingRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Logo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Desc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sex = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", maxLength: 100, nullable: true),
                    Birthday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Variety = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cow", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CowFeed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FoodType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FoodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FoodSource = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    FeedingTime = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 255, nullable: false),
                    FeedingSession = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CowFeed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CowStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<int>(type: "integer", nullable: false),
                    OldStatus = table.Column<string>(type: "text", nullable: false),
                    NewStatus = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CowStatusHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CowWeight",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<int>(type: "integer", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    MeasuredBy = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CowWeight", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    ExpectedCalvingDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: true),
                    ActualCalvingDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HouseCow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HouseName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseCow", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Houses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Houses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MilkBacth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CollectedFrom = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    CollectedTo = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: false),
                    TotalVolumeLiters = table.Column<decimal>(type: "numeric", maxLength: 255, nullable: false),
                    AvgFatPercent = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    AvgSnfPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TankNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkBacth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PregnancyCheck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<int>(type: "integer", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPregnant = table.Column<bool>(type: "boolean", nullable: false),
                    ExpectedCalvingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Method = table.Column<string>(type: "text", nullable: true),
                    PerformedBy = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PregnancyCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingProduction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CollectedFrom = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    CollectedTo = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: false),
                    TotalVolumeLiters = table.Column<decimal>(type: "numeric", maxLength: 255, nullable: false),
                    AvgFatPercent = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    AvgSnfPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TankNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingProduction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<string>(type: "text", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Shift = table.Column<string>(type: "text", nullable: false),
                    MilkQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    MilkFat = table.Column<decimal>(type: "numeric", nullable: false),
                    MilkSNF = table.Column<decimal>(type: "numeric", nullable: false),
                    MilkQualityGrade = table.Column<string>(type: "text", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    CollectedBy = table.Column<string>(type: "text", nullable: true),
                    BatchNumber = table.Column<string>(type: "text", nullable: true),
                    TransferredToTank = table.Column<bool>(type: "boolean", nullable: false),
                    TankNumber = table.Column<string>(type: "text", nullable: true),
                    IsAntibioticRestrictedMilk = table.Column<bool>(type: "boolean", nullable: false),
                    CowHealthStatus = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Production", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CollectedFrom = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    CollectedTo = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: false),
                    TotalVolumeLiters = table.Column<decimal>(type: "numeric", maxLength: 255, nullable: false),
                    AvgFatPercent = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    AvgSnfPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TankNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quality", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawMilk",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CowId = table.Column<string>(type: "text", nullable: true),
                    CollectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Shift = table.Column<string>(type: "text", nullable: true),
                    QtyLiters = table.Column<decimal>(type: "numeric", nullable: false),
                    CollectorName = table.Column<string>(type: "text", nullable: true),
                    MilkingMethod = table.Column<string>(type: "text", nullable: true),
                    MilkingStation = table.Column<string>(type: "text", nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric", nullable: false),
                    FatPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    SNFPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    Density = table.Column<decimal>(type: "numeric", nullable: false),
                    ProteinPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    LactosePercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Acidity = table.Column<decimal>(type: "numeric", nullable: false),
                    FreezingPoint = table.Column<decimal>(type: "numeric", nullable: false),
                    AddedWaterPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    BacteriaCount = table.Column<int>(type: "integer", nullable: false),
                    SomaticCellCount = table.Column<int>(type: "integer", nullable: false),
                    IsAntibioticPositive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSpoiled = table.Column<bool>(type: "boolean", nullable: false),
                    OdorCheck = table.Column<string>(type: "text", nullable: true),
                    ColorCheck = table.Column<string>(type: "text", nullable: true),
                    ContaminationNotes = table.Column<string>(type: "text", nullable: true),
                    RatePerLiter = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    SupplierId = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<string>(type: "text", nullable: true),
                    BatchNumber = table.Column<string>(type: "text", nullable: true),
                    TransferredToBulkTank = table.Column<bool>(type: "boolean", nullable: false),
                    BulkTankNumber = table.Column<string>(type: "text", nullable: true),
                    TransferTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoolingTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: true),
                    CowHealthStatus = table.Column<string>(type: "text", nullable: true),
                    UdderCondition = table.Column<string>(type: "text", nullable: true),
                    VeterinaryNotes = table.Column<string>(type: "text", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMilk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sex = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Birthday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Variety = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CollectedFrom = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    CollectedTo = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 20, nullable: false),
                    TotalVolumeLiters = table.Column<decimal>(type: "numeric", maxLength: 255, nullable: false),
                    AvgFatPercent = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: true),
                    AvgSnfPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TankNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Storage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetId = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    TestedAt = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 255, nullable: false),
                    TestType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", maxLength: 200, nullable: false),
                    ResultJson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Analyst = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trakuons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CropName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 100, nullable: false),
                    AgeDays = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    CirclePlanting = table.Column<int>(type: "integer", maxLength: 255, nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 200, nullable: false),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trakuons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sheep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sex = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Birthday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Variety = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HouseId = table.Column<int>(type: "integer", nullable: true),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheep_Houses_HouseId",
                        column: x => x.HouseId,
                        principalTable: "Houses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sheep_HouseId",
                table: "Sheep",
                column: "HouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Breeding");

            migrationBuilder.DropTable(
                name: "BreedingRecord");

            migrationBuilder.DropTable(
                name: "CalvingRecord");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Cow");

            migrationBuilder.DropTable(
                name: "CowFeed");

            migrationBuilder.DropTable(
                name: "CowStatusHistory");

            migrationBuilder.DropTable(
                name: "CowWeight");

            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "HouseCow");

            migrationBuilder.DropTable(
                name: "MilkBacth");

            migrationBuilder.DropTable(
                name: "PregnancyCheck");

            migrationBuilder.DropTable(
                name: "ProcessingProduction");

            migrationBuilder.DropTable(
                name: "Production");

            migrationBuilder.DropTable(
                name: "Quality");

            migrationBuilder.DropTable(
                name: "RawMilk");

            migrationBuilder.DropTable(
                name: "Salad");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Sheep");

            migrationBuilder.DropTable(
                name: "Storage");

            migrationBuilder.DropTable(
                name: "Trakuons");

            migrationBuilder.DropTable(
                name: "Houses");
        }
    }
}
