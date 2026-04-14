using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class Farming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    BranchCode = table.Column<string>(type: "text", nullable: false),
                    BranchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_Branch", x => x.BranchCode);
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
                name: "GoodsReceipt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentNumber = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VendorTax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalBeforeDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_GoodsReceipt", x => x.Id);
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
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InQty = table.Column<decimal>(type: "numeric", maxLength: 255, nullable: false),
                    OutQty = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UomCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceList = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Types = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
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
                    table.PrimaryKey("PK_ItemsMaster", x => x.Id);
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
                name: "sale_invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemsName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisPct = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceBfDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtVat = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvQty = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    LineStatus = table.Column<decimal>(type: "numeric", nullable: false),
                    ItemsCost = table.Column<decimal>(type: "numeric", nullable: false),
                    GTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    GrossPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: false),
                    UomName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Price = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PriceList = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Types = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FreeTxt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_sale_invoices", x => x.Id);
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
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StaffCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InActive = table.Column<bool>(type: "boolean", maxLength: 200, nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    TransId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "text", nullable: false),
                    TransType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Qty = table.Column<decimal>(type: "numeric", maxLength: 200, nullable: false),
                    TransDate = table.Column<DateTime>(type: "timestamp with time zone", maxLength: 50, nullable: false),
                    RefNo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FromWhs = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ToWhs = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_StockMovement", x => x.TransId);
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
                name: "UserMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MidName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tel1 = table.Column<string>(type: "text", nullable: false),
                    Tel2 = table.Column<string>(type: "text", nullable: true),
                    MobilePhone = table.Column<string>(type: "text", nullable: true),
                    TypeStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ActiveStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
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
                    table.PrimaryKey("PK_UserMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    WhsCode = table.Column<string>(type: "text", nullable: false),
                    WhsName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_Warehouse", x => x.WhsCode);
                });

            migrationBuilder.CreateTable(
                name: "Zo1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemsCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemsName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisPct = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceBfDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtDis = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAtVat = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvQty = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFC = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSys = table.Column<decimal>(type: "numeric", nullable: false),
                    LineStatus = table.Column<decimal>(type: "numeric", nullable: false),
                    ItemsCost = table.Column<decimal>(type: "numeric", nullable: false),
                    GTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    GrossPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: false),
                    UomName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Price = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PriceList = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Types = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FreeTxt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Zo1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoodsReceiptId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLine_GoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "GoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "UserAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserMasterId = table.Column<int>(type: "integer", nullable: false),
                    AddressType = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAddress_UserMaster_UserMasterId",
                        column: x => x.UserMasterId,
                        principalTable: "UserMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBankAcct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserMasterId = table.Column<int>(type: "integer", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    BankAcct = table.Column<string>(type: "text", nullable: false),
                    BankCode = table.Column<string>(type: "text", nullable: false),
                    BankType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBankAcct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBankAcct_UserMaster_UserMasterId",
                        column: x => x.UserMasterId,
                        principalTable: "UserMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserMasterId = table.Column<int>(type: "integer", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserContact_UserMaster_UserMasterId",
                        column: x => x.UserMasterId,
                        principalTable: "UserMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLine_GoodsReceiptId",
                table: "GoodsReceiptLine",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsMaster_ItemCode",
                table: "ItemsMaster",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheep_HouseId",
                table: "Sheep",
                column: "HouseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddress_UserMasterId",
                table: "UserAddress",
                column: "UserMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankAcct_UserMasterId",
                table: "UserBankAcct",
                column: "UserMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserContact_UserMasterId",
                table: "UserContact",
                column: "UserMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_Code",
                table: "UserMaster",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branch");

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
                name: "GoodsReceiptLine");

            migrationBuilder.DropTable(
                name: "HouseCow");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemsMaster");

            migrationBuilder.DropTable(
                name: "PregnancyCheck");

            migrationBuilder.DropTable(
                name: "Salad");

            migrationBuilder.DropTable(
                name: "sale_invoices");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Sheep");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "StockMovement");

            migrationBuilder.DropTable(
                name: "Storage");

            migrationBuilder.DropTable(
                name: "Trakuons");

            migrationBuilder.DropTable(
                name: "UserAddress");

            migrationBuilder.DropTable(
                name: "UserBankAcct");

            migrationBuilder.DropTable(
                name: "UserContact");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropTable(
                name: "Zo1");

            migrationBuilder.DropTable(
                name: "GoodsReceipt");

            migrationBuilder.DropTable(
                name: "Houses");

            migrationBuilder.DropTable(
                name: "UserMaster");
        }
    }
}
