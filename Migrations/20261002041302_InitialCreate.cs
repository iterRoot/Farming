using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Farming.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KACT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityNo = table.Column<int>(type: "integer", nullable: false),
                    ActivityKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BPCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelephoneNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedToType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedToCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssignedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPersonal = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MeetingLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Recurrence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReminderEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderMinutesBefore = table.Column<int>(type: "integer", nullable: false),
                    IsInactive = table.Column<bool>(type: "boolean", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResourceNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResourceActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CostItem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FinancialProject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProjectNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubprojectNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LinkDraft = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceObjectType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceObjectNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShowDocsRelatedToBP = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousActivityNo = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KACT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KADF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KADF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KAFF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StaffCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActiveStatus = table.Column<string>(type: "text", nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KAFF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KAPT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Operator = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KAPT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KAST",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ForeignName = table.Column<string>(type: "text", nullable: true),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    ItemGroup = table.Column<string>(type: "text", nullable: true),
                    UomGroup = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    PriceList = table.Column<string>(type: "text", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WithholdingTaxLiable = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotApplyDiscountGroups = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotApplyDiscount = table.Column<bool>(type: "boolean", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: true),
                    AdditionalIdentifier = table.Column<string>(type: "text", nullable: true),
                    ShippingType = table.Column<string>(type: "text", nullable: true),
                    ManageItemBy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedToResource = table.Column<string>(type: "text", nullable: true),
                    StandardItemIdentification = table.Column<string>(type: "text", nullable: true),
                    CommodityClassification = table.Column<string>(type: "text", nullable: true),
                    AssetClass = table.Column<string>(type: "text", nullable: true),
                    AssetGroup = table.Column<string>(type: "text", nullable: true),
                    CapitalizationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    RemainingLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    DepreciationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SalvageValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AssetStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DepreciationAccount = table.Column<string>(type: "text", nullable: true),
                    AccumulatedDepreciationAccount = table.Column<string>(type: "text", nullable: true),
                    AssetAccount = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KAST", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KATF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StoredName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AttachmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TargetPath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_KATF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KATT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CheckIn = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CheckOut = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    HoursWorked = table.Column<decimal>(type: "numeric(9,2)", nullable: true),
                    OvertimeHours = table.Column<decimal>(type: "numeric(9,2)", nullable: true),
                    LeaveType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KATT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBAT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BatchNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KBAT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBCD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UoMGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BarCodeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Sale"),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KBCD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBGT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BudgetName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "D"),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KBGT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBNR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReconcNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    StatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    GlOpeningBal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GlClosingBal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankOpeningBal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankClosingBal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReconciledAmt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnreconciledAmt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KBNR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBOM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ParentItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Warehouse = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KBOM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBPP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyNo = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AppliesTo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KBPP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBRN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BranchNameForeign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BranchRegNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AddressForeign = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MainBranch = table.Column<bool>(type: "boolean", nullable: false),
                    TaxOfficeNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultCustomerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultSupplierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultWarehouseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultResourceWarehouseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AliasName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StreetNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BuildingFloorRoom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Postcode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Block = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    County = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryRegion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GlobalLocationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_KBRN", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KBTX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BatchNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: false),
                    BaseDocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KBTX", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCCD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemClass = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    AlertType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountFrequency = table.Column<int>(type: "integer", nullable: false),
                    WarehouseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ItemGroupCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCCD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCDF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocalCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SystemCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ConversionRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    AccountFrom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountTo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalGain = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalLoss = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetDifference = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCDF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCLY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceDocEntry = table.Column<int>(type: "integer", nullable: false),
                    SourceDocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InitialQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainingQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KCLY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCMG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CommissionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CommissionRate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MinSaleAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxCommission = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCMG", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCMP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CampaignName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CampaignType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Planned"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KCMP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCND",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JobPostingId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Stage = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Applied"),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmployeeId = table.Column<int>(type: "integer", nullable: true),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_KCND", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCOA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcctCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcctName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FrgnName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    FatherNum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcctType = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    IsGroup = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Balance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    DebitBalance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    CreditBalance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    AllowPosting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BlockManualPosting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CashFlowRelevant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CashBox = table.Column<bool>(type: "boolean", nullable: false),
                    Confidential = table.Column<bool>(type: "boolean", nullable: false),
                    ControlAccount = table.Column<bool>(type: "boolean", nullable: false),
                    CashAccount = table.Column<bool>(type: "boolean", nullable: false),
                    Indexed = table.Column<bool>(type: "boolean", nullable: false),
                    RevalCurrency = table.Column<bool>(type: "boolean", nullable: false),
                    TaxIncome = table.Column<bool>(type: "boolean", nullable: false),
                    ExmIncome = table.Column<bool>(type: "boolean", nullable: false),
                    DfltVat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Frozen = table.Column<bool>(type: "boolean", nullable: false),
                    FrozenFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FrozenTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Project = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PrjRelvnt = table.Column<bool>(type: "boolean", nullable: false),
                    Dim1Relvnt = table.Column<bool>(type: "boolean", nullable: false),
                    Dim2Relvnt = table.Column<bool>(type: "boolean", nullable: false),
                    Dim3Relvnt = table.Column<bool>(type: "boolean", nullable: false),
                    Dim4Relvnt = table.Column<bool>(type: "boolean", nullable: false),
                    Dim5Relvnt = table.Column<bool>(type: "boolean", nullable: false),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClosingAcc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataSource = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true, defaultValue: "O"),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KCOA", x => x.Id);
                    table.UniqueConstraint("AK_KCOA_AcctCode", x => x.AcctCode);
                    table.ForeignKey(
                        name: "FK_KCOA_KCOA_FatherNum",
                        column: x => x.FatherNum,
                        principalTable: "KCOA",
                        principalColumn: "AcctCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KCOE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    BatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCOE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCOM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Logo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Desc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalCompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalStreet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalStreetNo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalBlock = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalBuildingFloorRoom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalCity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalPostcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalCounty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalState = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalCountryRegion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalInternetAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalPrintingHeader = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalActiveManager = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalAliasName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalFax = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalGln = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignCompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignStreet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignStreetNo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignBlock = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignBuildingFloorRoom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignCity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignPostcode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignCounty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignState = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignCountryRegion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignInternetAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignPrintingHeader = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignActiveManager = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignAliasName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignFax = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignGln = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxOffice = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VatNumber1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VatNumber2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VatNumber3 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompanyRegNo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompanyTaxRate = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    ExemptionNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxDeductionNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxOfficial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UseDeferredTax = table.Column<bool>(type: "boolean", nullable: false),
                    ApplyExchangeRateOnDeferredTax = table.Column<bool>(type: "boolean", nullable: false),
                    TaxRateDetermination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Holidays = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExtendedTaxReporting = table.Column<bool>(type: "boolean", nullable: false),
                    EoriNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AllowExternalTaxCalculationOnAR = table.Column<bool>(type: "boolean", nullable: false),
                    ChartOfAccountsTemplate = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalCurrency = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SystemCurrency = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultAccountCurrency = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayCreditBalanceNegative = table.Column<bool>(type: "boolean", nullable: false),
                    UseSegmentationAccounts = table.Column<bool>(type: "boolean", nullable: false),
                    AllowNegativeReversalPosting = table.Column<bool>(type: "boolean", nullable: false),
                    PermitMoreThanOneDocumentType = table.Column<bool>(type: "boolean", nullable: false),
                    MultiLanguageSupport = table.Column<bool>(type: "boolean", nullable: false),
                    UseContinuousStock = table.Column<bool>(type: "boolean", nullable: false),
                    ItemGroupsValuationMethod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ManageItemCostPerWarehouse = table.Column<bool>(type: "boolean", nullable: false),
                    UsePurchaseAccountsPosting = table.Column<bool>(type: "boolean", nullable: false),
                    AllowStockReleaseWithoutItemCost = table.Column<bool>(type: "boolean", nullable: false),
                    ManageSerialBatchCostBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EnableSeparateNetGrossPrice = table.Column<bool>(type: "boolean", nullable: false),
                    OrderingParty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultBankCountry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultBank = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultAccountNo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InstallBankStatementProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    EnableFixedAssets = table.Column<bool>(type: "boolean", nullable: false),
                    CalculateDepreciationBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EnableMultipleBranches = table.Column<bool>(type: "boolean", nullable: false),
                    MaskCreditCardNumber = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAdvancedGLDetermination = table.Column<bool>(type: "boolean", nullable: false),
                    AllowAnyAccountTypeForRevenue = table.Column<bool>(type: "boolean", nullable: false),
                    EnableProjectManagement = table.Column<bool>(type: "boolean", nullable: false),
                    EnablePersonalDataProtection = table.Column<bool>(type: "boolean", nullable: false),
                    TermsAndConditionsFile = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_KCOM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCRP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElementKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ElementName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ElementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Placeholder = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemElement = table.Column<bool>(type: "boolean", nullable: false),
                    PreviewHint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCRP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Host = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Protocol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    VirtualDir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DbHost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DbPort = table.Column<int>(type: "integer", nullable: true),
                    DbName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DbUser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DbPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DbProvider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    ReportFolder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OutputFolder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxRows = table.Column<int>(type: "integer", nullable: false),
                    CacheEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CacheMinutes = table.Column<int>(type: "integer", nullable: false),
                    SmtpHost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: true),
                    SmtpUser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SmtpPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SmtpFromEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SmtpFromName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCRS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCST",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DutyRate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KCST", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KCUR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CurrencyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IntlCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IntlDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HundredthName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    English = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EnglishHundredthName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsoCurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IncomingAmtDiff = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OutgoingAmtDiff = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IncomingPctDiff = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    OutgoingPctDiff = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Rounding = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Decimals = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RoundingInPayment = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KCUR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_KDDS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDEP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DepositDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Draft"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KDEP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDGS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_KDGS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDIE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Direction = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "I"),
                    EntityKey = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EntityLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    Format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "CSV"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "A"),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "P"),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    SuccessRows = table.Column<int>(type: "integer", nullable: false),
                    FailedRows = table.Column<int>(type: "integer", nullable: false),
                    SkippedRows = table.Column<int>(type: "integer", nullable: false),
                    ErrorLog = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KDIE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDNR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SeriesName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Primary"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    Prefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IncludeYear = table.Column<bool>(type: "boolean", nullable: false),
                    PadLength = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    FirstNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    NextNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LastNum = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_KDNR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDNT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KDNT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Posted"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KDPR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDRL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    InUse = table.Column<bool>(type: "boolean", nullable: false),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KDRL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDSH",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RoleTarget = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LayoutType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ThemeColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KDSH", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDSP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Options = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KDSP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KDUN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LetterNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DunningTermCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LetterText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RunDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalOverdue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KDUN", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KECT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CertNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertStandard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HolderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HolderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HolderName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    HolderDept = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IssuingCountry = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AuthorityContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RenewalDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RenewalDaysAlert = table.Column<int>(type: "integer", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RenewedFromId = table.Column<int>(type: "integer", nullable: true),
                    RenewedToId = table.Column<int>(type: "integer", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QrCodeData = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VerificationUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSigned = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureCert = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SignatureHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocId = table.Column<int>(type: "integer", nullable: true),
                    LinkedDocNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Scope = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CoveredSites = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Restrictions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KECT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KEDM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BaseDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseDocId = table.Column<int>(type: "integer", nullable: true),
                    BaseDocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseDocDate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BPCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BPTaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Encoding = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SchemaVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubmissionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QrCodeData = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResponseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsSigned = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureCert = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CertExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KEDM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KEFM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StorageProvider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LinkedModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocId = table.Column<int>(type: "integer", nullable: true),
                    LinkedDocNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedRoles = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UploadedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UploadedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    LastDownloadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentVersion = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KEFM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KEML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SmtpHost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    Encryption = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    UseAuth = table.Column<bool>(type: "boolean", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    FromEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FromName = table.Column<string>(type: "text", nullable: true),
                    ReplyTo = table.Column<string>(type: "text", nullable: true),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KEML", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KEMP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Branch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaritalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dependents = table.Column<int>(type: "integer", nullable: false),
                    Mobile = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Salary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BankName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    BankAccountNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KEMP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KERD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClosingRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    AccountFrom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountTo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalGain = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalLoss = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetDifference = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KERD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KFRT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReportType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KFRT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KFXS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutoFetchEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Currencies = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RunAtHourUtc = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KFXS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KGIE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Open"),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VendorTax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BaseDocId = table.Column<int>(type: "integer", nullable: true),
                    BaseDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalBeforeDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_KGIE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KGLD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountsReceivable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesRevenue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesDiscount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesTax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerDeposit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountsPayable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseExpense = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseDiscount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseTax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VendorDeposit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WithholdingTaxPayable = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CostOfGoodsSold = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryOffset = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryRevaluationIncrease = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryRevaluationDecrease = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryCountingGain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InventoryCountingLoss = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PriceDifference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CashAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExchangeGainLoss = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RetainedEarnings = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OpeningBalance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ARControlAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RevenueAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxOutputAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_KGLD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KGLO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Field = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_KGLO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KGRT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Open"),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentNumber = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VendorTax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalBeforeDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_KGRT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KGST",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_KGST", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KIMP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Phase = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Responsible = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PlannedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LinkPath = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KIMP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KINM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseDocType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: false),
                    BaseDocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OutQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TransValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QtyBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ValueBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PriceVariance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_KINM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KITB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultUom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KITB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KITG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyNo = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KITG", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KITM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemGroup = table.Column<string>(type: "text", nullable: false),
                    ItemType = table.Column<string>(type: "text", nullable: false),
                    Barcode = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    UomName = table.Column<string>(type: "text", nullable: false),
                    UomCode = table.Column<string>(type: "text", nullable: false),
                    ManageBy = table.Column<string>(type: "text", nullable: false),
                    SalesItem = table.Column<bool>(type: "boolean", nullable: false),
                    PurchaseItem = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Warehouse = table.Column<string>(type: "text", nullable: false),
                    MinStock = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxStock = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValuationMethod = table.Column<string>(type: "text", nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceList = table.Column<string>(type: "text", nullable: false),
                    SaleTaxGroup = table.Column<string>(type: "text", nullable: false),
                    SaleDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaleUom = table.Column<string>(type: "text", nullable: false),
                    SaleDescription = table.Column<string>(type: "text", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PreferredVendor = table.Column<string>(type: "text", nullable: false),
                    VendorItemNo = table.Column<string>(type: "text", nullable: false),
                    PurchaseUom = table.Column<string>(type: "text", nullable: false),
                    LeadTime = table.Column<int>(type: "integer", nullable: false),
                    PurchaseTaxGroup = table.Column<string>(type: "text", nullable: false),
                    ForeignName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UomGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PricingUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StockItem = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotApplyDiscountGroups = table.Column<bool>(type: "boolean", nullable: false),
                    DoNotApplyDiscount = table.Column<bool>(type: "boolean", nullable: false),
                    AdditionalIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StandardItemIdentification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CommodityClassification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MfrCatalogueNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchasingUomCode = table.Column<string>(type: "text", nullable: true),
                    PurchasingUomName = table.Column<string>(type: "text", nullable: true),
                    ItemsPerPurchasingUnit = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchasePackageType = table.Column<string>(type: "text", nullable: true),
                    PurchaseQtyPerPackage = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchaseLength = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchaseWidth = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchaseHeight = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchaseVolume = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PurchaseWeight = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    DutyGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseVatCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesVatCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SalesUomCode = table.Column<string>(type: "text", nullable: true),
                    SalesUomName = table.Column<string>(type: "text", nullable: true),
                    ItemsPerSalesUnit = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesPackageType = table.Column<string>(type: "text", nullable: true),
                    SalesQtyPerPackage = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesLength = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesWidth = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesHeight = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesVolume = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    SalesWeight = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    CreateQrCodeFrom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SetGlAccountsBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StockWeight = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    StockCountingUomCode = table.Column<string>(type: "text", nullable: true),
                    StockCountingUomName = table.Column<string>(type: "text", nullable: true),
                    ItemsPerCountingUnit = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ManageStockByWarehouse = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredStock = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PlanningMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcurementMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrderInterval = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrderMultiple = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    MinimumOrderQty = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    ToleranceDays = table.Column<int>(type: "integer", nullable: false),
                    PhantomItem = table.Column<bool>(type: "boolean", nullable: false),
                    IssueMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BomType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductionStdCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IncludeInProductionStdCostRollup = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KITM", x => x.Id);
                    table.CheckConstraint("CK_ItemsMaster_ItemCode_NotEmpty", "\"ItemCode\" <> ''");
                });

            migrationBuilder.CreateTable(
                name: "KIVY",
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
                    table.PrimaryKey("PK_KIVY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KJOB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostingNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmploymentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Open"),
                    NumberOfOpenings = table.Column<int>(type: "integer", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetCloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HiringManager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Requirements = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KJOB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KJRN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JrnlNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ref2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ref3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "JE"),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseDocType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    IsAutoGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    ProjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    UserSign2 = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KJRN", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KJVH",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VoucherNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "D"),
                    PostedToJournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<int>(type: "integer", nullable: true),
                    ProjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    UserSign2 = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ApprovalStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "N"),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KJVH", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KLCP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BillingCycle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Monthly"),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    Modules = table.Column<List<string>>(type: "text[]", nullable: false),
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
                    table.PrimaryKey("PK_KLCP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KLCTYP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KLCTYP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KLOC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WarehouseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Aisle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Row = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Shelf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Bin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Zone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxWeight = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KLOC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KLVR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LeaveType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Days = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    HalfDay = table.Column<bool>(type: "boolean", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DecidedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KLVR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KLWU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ConversionToMeter = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KLWU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KMAL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alias = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MenuName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MenuPath = table.Column<string>(type: "text", nullable: true),
                    Module = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KMAL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KMRC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KMRC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KMSP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EventKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DigestTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DigestDay = table.Column<int>(type: "integer", nullable: true),
                    EmailOverride = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneOverride = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WebhookUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebhookSecret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubjectTemplate = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    BodyTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConditionFilter = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KMSP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KOCR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeSeries = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FrgnName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AliasName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TypeStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ActiveStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveRemarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GroupCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    VatNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnifiedVatNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdNo2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tel1 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Tel2 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MobilePhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Fax = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FactoringIndicator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPProject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TypeOfBusiness = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DefaultContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BPChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Technician = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Territory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GLN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BranchAssignment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BlockMarketingContent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PayTerms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InterestOnArrears = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false, defaultValue: 0m),
                    PriceList = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalDiscount = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false, defaultValue: 0m),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CommitmentLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DunningTerm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveDiscountGroups = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectivePrice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreditCardType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreditCardNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreditCardExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreditCardIdNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AverageDelay = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultIban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Holidays = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentDates = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AllowPartialDeliveryOfSO = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowPartialDeliveryPerRow = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DoNotApplyDiscountGroups = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EndorsableCheques = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AcceptsEndorsedCheques = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HouseBankCountry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HouseBankCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    HouseBankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HouseBankBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HouseBankIban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HouseBankSwift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HouseBankControlNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceDetails = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentBlock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PaymentBlockCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SinglePayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CollectionAuthorisation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BankChargesAllocationCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AutoCalcBankCharges = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConsolidatingBP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsolidationType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ARControlAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DownPaymentClearingAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DownPaymentInterimAccount = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BlockDunningLetters = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DunningLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DunningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectedSupplier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PlanningGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UseShippedGoodsAccount = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Affiliate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DeliveriesBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OrdersBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OpportunitiesBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ChequesBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VatGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KOCR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KOPP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OpportunityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OpportunityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BPCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalAmountInvoiced = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BPTerritory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayInSystemCurrency = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenActivities = table.Column<int>(type: "integer", nullable: false),
                    ClosingPercent = table.Column<int>(type: "integer", nullable: false),
                    PredictedClosingIn = table.Column<int>(type: "integer", nullable: true),
                    PredictedClosingUnit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PredictedClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PotentialAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeightedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossProfitPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    GrossProfitTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LevelOfInterest = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BPChannelCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BPChannelName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BPChannelContact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BPProject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InformationSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocumentNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShowDocsRelatedToBP = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KOPP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KOPPCMP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Strengths = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Weaknesses = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KOPPCMP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KOPPRSN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KOPPRSN", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KOPPSTG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Probability = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KOPPSTG", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPAY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxableAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ExemptAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Bonus = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OtherDeductions = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Dependents = table.Column<int>(type: "integer", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnpaidLeaveDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TaxOnSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NssfEmployee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NssfEmployer = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KPAY", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPCK",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PickNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PickDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Picker = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Released"),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KPCK", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPKT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Material = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Length = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxWeight = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Tare = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DimUnit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    WeightUnit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KPKT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCenterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CenterType = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "D"),
                    ParentCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Manager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_KPRC", x => x.Id);
                    table.UniqueConstraint("AK_KPRC_CostCenterCode", x => x.CostCenterCode);
                    table.ForeignKey(
                        name: "FK_KPRC_KPRC_ParentCode",
                        column: x => x.ParentCode,
                        principalTable: "KPRC",
                        principalColumn: "CostCenterCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPRD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeriodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PeriodStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PostingDateFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    PostingDateTo = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDateFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDateTo = table.Column<DateOnly>(type: "date", nullable: true),
                    DocumentDateFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    DocumentDateTo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_KPRD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreateNextYearDueDates = table.Column<bool>(type: "boolean", nullable: false),
                    AutoUpdateStatus = table.Column<bool>(type: "boolean", nullable: false),
                    DaysAfterNewPeriod = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPRDS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IndexCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndexName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IndexValue = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KPRI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRJ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_KPRJ", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    Trigger = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Created = table.Column<int>(type: "integer", nullable: false),
                    Refreshed = table.Column<int>(type: "integer", nullable: false),
                    SkippedPaid = table.Column<int>(type: "integer", nullable: false),
                    SkippedNotEmployed = table.Column<int>(type: "integer", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalNssfEmployee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalNssfEmployer = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_KPRL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPRM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProjectType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "External"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Started"),
                    Series = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BpCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BpName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Territory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    WithSubprojects = table.Column<bool>(type: "boolean", nullable: false),
                    FinancialProject = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Industry = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenActivities = table.Column<int>(type: "integer", nullable: false),
                    PercentComplete = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
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
                    table.PrimaryKey("PK_KPRM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPTX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TextContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KPTX", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPWDPOL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinLength = table.Column<int>(type: "integer", nullable: false),
                    RequireUppercase = table.Column<bool>(type: "boolean", nullable: false),
                    RequireLowercase = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDigit = table.Column<bool>(type: "boolean", nullable: false),
                    RequireSpecial = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDays = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KPWDPOL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPYI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnappliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Cash"),
                    CheckNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Deposited = table.Column<bool>(type: "boolean", nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    TransType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "KPYI"),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    UserSign2 = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KPYI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPYO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnappliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Cash"),
                    CheckNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BankAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    TransType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "KPYO"),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    UserSign2 = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KPYO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KRFL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetTable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetKeyField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetDisplayField = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetFilter = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TargetOrderBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetApiEndpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LinkType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSearch = table.Column<bool>(type: "boolean", nullable: false),
                    AllowCreate = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AutoFillFields = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KRFL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KRLM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LayoutType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LayoutEngine = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastUploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HtmlTemplate = table.Column<string>(type: "text", nullable: true),
                    PaperSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Orientation = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    PageWidth = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    PageHeight = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    MarginTop = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    MarginBottom = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    MarginLeft = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    MarginRight = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    OutputFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AllowFormatChange = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Copies = table.Column<int>(type: "integer", nullable: false),
                    ShowWatermark = table.Column<bool>(type: "boolean", nullable: false),
                    WatermarkText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemLayout = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    AllowedRoles = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChangeLog = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KRLM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KRODBU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KRODBU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KRTT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CurrencyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KRTT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSCN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Active"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseTimeHours = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CoverageTerms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KSCN", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSEB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mobile = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SalesRegion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Territory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    CommissionGroupId = table.Column<int>(type: "integer", nullable: true),
                    CommissionGroupCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CommissionGroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KSEB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSHT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Carrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TrackingUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KSHT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSPC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrinterCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrinterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrinterType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NetworkPath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    Protocol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ShareName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DriverName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    QueueName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrintUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrintPassword = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PaperSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Orientation = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CustomWidth = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    CustomHeight = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    DefaultCopies = table.Column<int>(type: "integer", nullable: false),
                    ColorMode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DuplexMode = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CanColor = table.Column<bool>(type: "boolean", nullable: false),
                    CanDuplex = table.Column<bool>(type: "boolean", nullable: false),
                    CanStaple = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDpi = table.Column<int>(type: "integer", nullable: false),
                    SupportedPapers = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PrintLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KSPC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSPL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LinkType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IconName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BadgeText = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsEmbeddable = table.Column<bool>(type: "boolean", nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AuthType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApiKeyHeader = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SamplePayload = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedDocType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedRoles = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequiresVPN = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresLogin = table.Column<bool>(type: "boolean", nullable: false),
                    LoginHint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsOnDashboard = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ClickCount = table.Column<int>(type: "integer", nullable: false),
                    LastClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStatusCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsHealthy = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KSPL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSRCH",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TargetEntity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KSRCH", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSRV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Series = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevaluationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Ref2 = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    InflationBased = table.Column<bool>(type: "boolean", nullable: false),
                    ReferencedDocumentId = table.Column<int>(type: "integer", nullable: true),
                    ReferencedDocumentLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Posted"),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    JournalRemark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CostsApplied = table.Column<bool>(type: "boolean", nullable: false),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KSRV", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSTC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CycleType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarehouseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResponsibleBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KSTC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Series = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Primary"),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShipTo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FromWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "01"),
                    ToWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "01"),
                    ToBinLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PriceList = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferencedDoc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseDocType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JournalRemarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KSTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSTR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Series = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Primary"),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ShipTo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FromWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "01"),
                    ToWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "01"),
                    PriceList = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferencedDoc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseDocType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JournalRemarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PickPackRemarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_KSTR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KSVC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceCallNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProblemDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Open"),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Medium"),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OpenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ServiceContractId = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KSVC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KTAX",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcquisitionReverse = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    NonDeductiblePercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    TaxAccount = table.Column<string>(type: "text", nullable: true),
                    AcquisitionTaxAccount = table.Column<string>(type: "text", nullable: true),
                    DeferredTaxAccount = table.Column<string>(type: "text", nullable: true),
                    NonDeductibleAccount = table.Column<string>(type: "text", nullable: true),
                    CodeDescription = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KTAX", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KTER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    ParentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ManagerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TargetRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KTER", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KTER_KTER_ParentId",
                        column: x => x.ParentId,
                        principalTable: "KTER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KTXD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BusinessArea = table.Column<string>(type: "text", nullable: true),
                    Condition1 = table.Column<string>(type: "text", nullable: true),
                    Value1 = table.Column<string>(type: "text", nullable: true),
                    Condition2 = table.Column<string>(type: "text", nullable: true),
                    Value2 = table.Column<string>(type: "text", nullable: true),
                    Condition3 = table.Column<string>(type: "text", nullable: true),
                    Value3 = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LineTaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KTXD", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUAU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FullAuthorization = table.Column<bool>(type: "boolean", nullable: false),
                    Permissions = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_KUAU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUDF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultWarehouse = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultPriceList = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultPaymentTerms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultShippingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DefaultGLAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultCostCentre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultProject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultBranch = table.Column<int>(type: "integer", nullable: true),
                    DefaultSalesEmployee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultSalesRegion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultARSeries = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultCustomerGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultBuyer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DefaultAPSeries = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultVendorGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultPrinter = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DecimalSeparator = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ThousandSeparator = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Theme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ShowTutorials = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmOnClose = table.Column<bool>(type: "boolean", nullable: false),
                    AutoSave = table.Column<bool>(type: "boolean", nullable: false),
                    AutoSaveMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KUDF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUDT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_KUDT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUDV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KUDV", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUGP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BaseUom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KUGP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUGR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GroupType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KUGR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUOM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DecimalPlaces = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KUOM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KUSG",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GroupDec = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Allowences = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TPLId = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GroupType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CockpitId = table.Column<int>(type: "integer", nullable: true),
                    LogInstanc = table.Column<int>(type: "integer", nullable: true),
                    UserSign = table.Column<int>(type: "integer", nullable: true),
                    UserSign2 = table.Column<int>(type: "integer", nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_KUSG", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KWHS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WhsCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WhsName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Manager = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Branch = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Street = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    State = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Nettable = table.Column<bool>(type: "boolean", nullable: false),
                    DropShip = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_KWHS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KWHT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    BaseType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoundingType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BaseAmountPercent = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                    OfficialCode = table.Column<string>(type: "text", nullable: true),
                    Account = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KWHT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KWOR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BillOfMaterialId = table.Column<int>(type: "integer", nullable: false),
                    BomCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PlannedQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Warehouse = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "Planned"),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KWOR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KWTU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ConversionToKg = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KWTU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OPKL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriteriaName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GroupBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReleaseManually = table.Column<bool>(type: "boolean", nullable: false),
                    ManageSalesOrders = table.Column<bool>(type: "boolean", nullable: false),
                    ManageReserveInvoices = table.Column<bool>(type: "boolean", nullable: false),
                    ManageProductionOrders = table.Column<bool>(type: "boolean", nullable: false),
                    ManageStockTransferRequests = table.Column<bool>(type: "boolean", nullable: false),
                    Warehouses = table.Column<List<string>>(type: "text[]", nullable: false),
                    GuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InActive = table.Column<bool>(type: "boolean", nullable: true),
                    Filters = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OPKL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OPLN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MultipleBase = table.Column<bool>(type: "boolean", nullable: false),
                    BasePriceListId = table.Column<int>(type: "integer", nullable: true),
                    Factor = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    RoundingMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RoundingRule = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EndingInterval = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    AuthorizationGroup = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    GrossPrice = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OPLN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OPLN_OPLN_BasePriceListId",
                        column: x => x.BasePriceListId,
                        principalTable: "OPLN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ORCR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextExecution = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TimesExecuted = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ORCR", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OUDF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ValidValues = table.Column<List<string>>(type: "text[]", nullable: false),
                    LinkedTable = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OUDF", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KACA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    TargetPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AttachmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KACA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KACA_KACT_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "KACT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KAD1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressFormatId = table.Column<int>(type: "integer", nullable: false),
                    LineOrder = table.Column<int>(type: "integer", nullable: false),
                    Field1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Field2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Field3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Separator12 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Separator23 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PrintIfEmpty = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KAD1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KAD1_KADF_AddressFormatId",
                        column: x => x.AddressFormatId,
                        principalTable: "KADF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BCD1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BarCodeId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    UoM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FreeText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_BCD1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BCD1_KBCD_BarCodeId",
                        column: x => x.BarCodeId,
                        principalTable: "KBCD",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BGT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AcctCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AcctName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    AcctType = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A"),
                    Annual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Jan = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Feb = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Mar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Apr = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    May = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Jun = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Jul = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aug = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Sep = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Oct = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Nov = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dec = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Project = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_BGT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BGT1_KBGT_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "KBGT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BNR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankReconciliationId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    TransType = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "GL"),
                    TransDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DocEntry = table.Column<int>(type: "integer", nullable: true),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    ReconciledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReconciledRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_BNR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BNR1_KBNR_BankReconciliationId",
                        column: x => x.BankReconciliationId,
                        principalTable: "KBNR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOM1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillOfMaterialId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOM1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BOM1_KBOM_BillOfMaterialId",
                        column: x => x.BillOfMaterialId,
                        principalTable: "KBOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CDF1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversionDifferenceId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "GL"),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LocalBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HistoricalRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    CurrentRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    SystemBalanceBefore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SystemBalanceAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CDF1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CDF1_KCDF_ConversionDifferenceId",
                        column: x => x.ConversionDifferenceId,
                        principalTable: "KCDF",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KCM1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommissionGroupId = table.Column<int>(type: "integer", nullable: false),
                    TierOrder = table.Column<int>(type: "integer", nullable: false),
                    FromAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KCM1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KCM1_KCMG_CommissionGroupId",
                        column: x => x.CommissionGroupId,
                        principalTable: "KCMG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CMP1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    BPCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BPName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotContacted"),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CMP1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CMP1_KCMP_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "KCMP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "COE1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AcctCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AcctName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ChangeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NewValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OldValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Applied = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COE1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_COE1_KCOE_BatchId",
                        column: x => x.BatchId,
                        principalTable: "KCOE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DEP1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepositId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    IncomingPaymentId = table.Column<int>(type: "integer", nullable: false),
                    PaymentDocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HoldingAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEP1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DEP1_KDEP_DepositId",
                        column: x => x.DepositId,
                        principalTable: "KDEP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DNT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DunningTermId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    DaysOverdue = table.Column<int>(type: "integer", nullable: false),
                    LetterText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DNT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DNT1_KDNT_DunningTermId",
                        column: x => x.DunningTermId,
                        principalTable: "KDNT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DPR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepreciationRunId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingLifeBefore = table.Column<int>(type: "integer", nullable: false),
                    RemainingLifeAfter = table.Column<int>(type: "integer", nullable: false),
                    StatusBefore = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpenseAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccumulatedAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPR1_KDPR_DepreciationRunId",
                        column: x => x.DepreciationRunId,
                        principalTable: "KDPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DRL1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistributionRuleId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    CostCenterCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CostCenterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    Percentage = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_DRL1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DRL1_KDRL_DistributionRuleId",
                        column: x => x.DistributionRuleId,
                        principalTable: "KDRL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KDS1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DashboardConfigId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    WidgetKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WidgetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ColSpan = table.Column<int>(type: "integer", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Settings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KDS1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KDS1_KDSH_DashboardConfigId",
                        column: x => x.DashboardConfigId,
                        principalTable: "KDSH",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DUN1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DunningLetterId = table.Column<int>(type: "integer", nullable: false),
                    ARInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    DocNum = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DaysOverdue = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DUN1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DUN1_KDUN_DunningLetterId",
                        column: x => x.DunningLetterId,
                        principalTable: "KDUN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KECA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectronicCertificateId = table.Column<int>(type: "integer", nullable: false),
                    AttrKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttrLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AttrValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KECA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KECA_KECT_ElectronicCertificateId",
                        column: x => x.ElectronicCertificateId,
                        principalTable: "KECT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KED1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectronicDocumentId = table.Column<int>(type: "integer", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ExternalRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KED1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KED1_KEDM_ElectronicDocumentId",
                        column: x => x.ElectronicDocumentId,
                        principalTable: "KEDM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KEFV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectronicFileId = table.Column<int>(type: "integer", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KEFV", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KEFV_KEFM_ElectronicFileId",
                        column: x => x.ElectronicFileId,
                        principalTable: "KEFM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ERD1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExchangeRateDifferenceId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "GL"),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ForeignBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HistoricalRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    CurrentRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    LocalBalanceBefore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LocalBalanceAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ERD1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ERD1_KERD_ExchangeRateDifferenceId",
                        column: x => x.ExchangeRateDifferenceId,
                        principalTable: "KERD",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FRT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    RowCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RowType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Account"),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ParentRowCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountFrom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountTo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Formula = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Sign = table.Column<int>(type: "integer", nullable: false),
                    Bold = table.Column<bool>(type: "boolean", nullable: false),
                    Underline = table.Column<bool>(type: "boolean", nullable: false),
                    PrintZero = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FRT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FRT1_KFRT_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "KFRT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GIE1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoodsIssueId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GIE1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GIE1_KGIE_GoodsIssueId",
                        column: x => x.GoodsIssueId,
                        principalTable: "KGIE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GRT1",
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
                    table.PrimaryKey("PK_GRT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GRT1_KGRT_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "KGRT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JRN1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalEntryId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DebitFC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditFC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DebitLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditLC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineMemo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Project = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_JRN1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JRN1_KJRN_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "KJRN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JVH1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalVoucherId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CardCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DebitFC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditFC = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineMemo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Project = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_JVH1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JVH1_KJVH_JournalVoucherId",
                        column: x => x.JournalVoucherId,
                        principalTable: "KJVH",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KABA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KABA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KABA_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KACN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KACN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KACN_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KADI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KADI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KADI_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KADR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DownPaymentPct = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DownPaymentAmt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KADR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KADR_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KADV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KADV", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KADV_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KARE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KARE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KARE_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KARI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShipToAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    BillToAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ShippingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PickAndPackRemarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentTerms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CashDiscountDateOffset = table.Column<int>(type: "integer", nullable: true),
                    BPProject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ControlAccount = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JournalRemark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KARI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KARI_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KARR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KARR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KARR_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KARV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KARV", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KARV_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KASO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KASO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KASO_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KASQ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KASQ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KASQ_KOCR_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KBPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReconcNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    PartnerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PartnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReconciledAmt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnreconciledAmt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VersionNum = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KBPR", x => x.Id);
                    table.CheckConstraint("CK_BPReconciliation_Status", "\"Status\" IN ('O','C')");
                    table.ForeignKey(
                        name: "FK_KBPR_KOCR_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KCPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BPId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Tel1 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Tel2 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MobilePhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Fax = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmailGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Pager = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Remarks1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remarks2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryOfBirth = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Profession = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CityOfBirth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConnectedAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BlockMarketingContent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EDocRecipient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_KCPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KCPR_KOCR_BPId",
                        column: x => x.BPId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KCRB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BPId = table.Column<int>(type: "integer", nullable: false),
                    BankCountry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SwiftNum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Branch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CtrlIntId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MandateId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfSignature = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_KCRB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KCRB_KOCR_BPId",
                        column: x => x.BPId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KCRD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BPId = table.Column<int>(type: "integer", nullable: false),
                    AdresType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AdressName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressName2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressName3 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    StreetNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Block = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BuildingFloorRoom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    County = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxOffice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GLN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_KCRD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KCRD_KOCR_BPId",
                        column: x => x.BPId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KLDC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KLDC", x => x.Id);
                    table.CheckConstraint("CK_LandedCost_Status", "\"Status\" IN ('D','O')");
                    table.ForeignKey(
                        name: "FK_KLDC_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPAC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPAC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPAC_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPAI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPAI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPAI_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPAR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPAR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPAR_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPBA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPBA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPBA_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPGQ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPGQ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPGQ_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPGR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_KPGR", x => x.Id);
                    table.CheckConstraint("CK_GRPO_Status", "\"Status\" IN ('O','C','D','P')");
                    table.ForeignKey(
                        name: "FK_KPGR_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPGT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPGT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPGT_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPMT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BPId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Include = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_KPMT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPMT_KOCR_BPId",
                        column: x => x.BPId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPPO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPPO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPPO_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPPQ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_KPPQ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPPQ_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: true),
                    RequestedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NeededByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KPPR", x => x.Id);
                    table.CheckConstraint("CK_PurchaseRequest_Status", "\"Status\" IN ('O','C','D','P')");
                    table.ForeignKey(
                        name: "FK_KPPR_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KOPA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    TargetPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AttachmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPA_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KOPC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThreatLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Won = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPC_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KOPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPI_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KOPN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPN_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KOPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedBP = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPR_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KOPS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesEmployee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    PotentialAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeightedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ShowBPsDocs = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KOPS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KOPS_KOPP_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "KOPP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PCK1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PickListId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    TransType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DocNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BpCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliveryDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ItemNo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UomName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OrderQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OpenQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PickedQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_PCK1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PCK1_KPCK_PickListId",
                        column: x => x.PickListId,
                        principalTable: "KPCK",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Task = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UniqueId = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Assignee = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    PercentComplete = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPRS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPRS_KPRM_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "KPRM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PYI1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncomingPaymentId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceDocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceDocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_PYI1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PYI1_KPYI_IncomingPaymentId",
                        column: x => x.IncomingPaymentId,
                        principalTable: "KPYI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PYO1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutgoingPaymentId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceDocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceDocDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PYO1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PYO1_KPYO_OutgoingPaymentId",
                        column: x => x.OutgoingPaymentId,
                        principalTable: "KPYO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KRLP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportLayoutId = table.Column<int>(type: "integer", nullable: false),
                    ParamOrder = table.Column<int>(type: "integer", nullable: false),
                    ParamKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParamLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Options = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KRLP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KRLP_KRLM_ReportLayoutId",
                        column: x => x.ReportLayoutId,
                        principalTable: "KRLM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KSPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerPrintConfigId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Copies = table.Column<int>(type: "integer", nullable: false),
                    PaperTray = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaperSize = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KSPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KSPR_KSPC_ServerPrintConfigId",
                        column: x => x.ServerPrintConfigId,
                        principalTable: "KSPC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KSPU",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SAPLinkId = table.Column<int>(type: "integer", nullable: false),
                    ClickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Referrer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KSPU", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KSPU_KSPL_SAPLinkId",
                        column: x => x.SAPLinkId,
                        principalTable: "KSPL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SRC1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SavedSearchId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    Field = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Operator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SRC1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SRC1_KSRCH_SavedSearchId",
                        column: x => x.SavedSearchId,
                        principalTable: "KSRCH",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SRV1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockRevaluationId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ItemDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CurrentCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    NewCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Uom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InStock = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_SRV1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SRV1_KSRV_StockRevaluationId",
                        column: x => x.StockRevaluationId,
                        principalTable: "KSRV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SRV2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockRevaluationId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    SbNo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    NewCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OpenQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_SRV2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SRV2_KSRV_StockRevaluationId",
                        column: x => x.StockRevaluationId,
                        principalTable: "KSRV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STC1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockCycleId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Uom = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SystemQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CountedQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PostedQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PostedUnitCost = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STC1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_STC1_KSTC_StockCycleId",
                        column: x => x.StockCycleId,
                        principalTable: "KSTC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STO1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockTransferId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FromWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FromBinLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ToBinLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UoMCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UoMName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FirstPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_STO1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_STO1_KSTO_StockTransferId",
                        column: x => x.StockTransferId,
                        principalTable: "KSTO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StockTransferRequestId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FromWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ToWarehouse = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UoMCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UoMName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LineStatus = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "O"),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_STR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_STR1_KSTR_StockTransferRequestId",
                        column: x => x.StockTransferRequestId,
                        principalTable: "KSTR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KUTF",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserTableId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    FieldCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ValidValues = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KUTF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KUTF_KUDT_UserTableId",
                        column: x => x.UserTableId,
                        principalTable: "KUDT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KUTR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserTableId = table.Column<int>(type: "integer", nullable: false),
                    RowKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RowName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Data = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KUTR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KUTR_KUDT_UserTableId",
                        column: x => x.UserTableId,
                        principalTable: "KUDT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UGP1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UomGroupId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AltUom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AltQty = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    BaseQty = table.Column<decimal>(type: "numeric(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UGP1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UGP1_KUGP_UomGroupId",
                        column: x => x.UomGroupId,
                        principalTable: "KUGP",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KUG1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGroupId = table.Column<int>(type: "integer", nullable: false),
                    UserCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsGroupAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KUG1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KUG1_KUGR_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "KUGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KUG2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserGroupId = table.Column<int>(type: "integer", nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubModule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanRead = table.Column<bool>(type: "boolean", nullable: false),
                    CanUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false),
                    CanExport = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KUG2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KUG2_KUGR_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "KUGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KUSR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserGroupId = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActiveStatus = table.Column<char>(type: "char(1)", nullable: false, defaultValue: 'A'),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorSecret = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorRecoveryCodes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KUSR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KUSR_KUSG_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "KUSG",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WOR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductionOrderId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlannedQty = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Warehouse = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WOR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WOR1_KWOR_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "KWOR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PKL1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PickPackCriteriaId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    BaseDocType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BaseDocEntry = table.Column<int>(type: "integer", nullable: false),
                    BaseLineId = table.Column<int>(type: "integer", nullable: true),
                    BaseDocNum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequiredQty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PickedQty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_PKL1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PKL1_OPKL_PickPackCriteriaId",
                        column: x => x.PickPackCriteriaId,
                        principalTable: "OPKL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KIPL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PriceListId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_KIPL", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KIPL_OPLN_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "OPLN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RCR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecurringPostingId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineMemo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_RCR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RCR1_ORCR_RecurringPostingId",
                        column: x => x.RecurringPostingId,
                        principalTable: "ORCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ACN1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ARCreditNoteId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ACN1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ACN1_KACN_ARCreditNoteId",
                        column: x => x.ARCreditNoteId,
                        principalTable: "KACN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ACN1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADI1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ARDownPaymentInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ADI1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ADI1_KADI_ARDownPaymentInvoiceId",
                        column: x => x.ARDownPaymentInvoiceId,
                        principalTable: "KADI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ADI1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArDownPaymentRequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ADR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ADR1_KADR_ArDownPaymentRequestId",
                        column: x => x.ArDownPaymentRequestId,
                        principalTable: "KADR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ADR1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADV1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeliveryId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ADV1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ADV1_KADV_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "KADV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ADV1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ARE1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReturnId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ARE1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARE1_KARE_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "KARE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ARE1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ABA1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleBlanketAgreementId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ARInvoiceId = table.Column<int>(type: "integer", nullable: true),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ABA1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ABA1_KABA_SaleBlanketAgreementId",
                        column: x => x.SaleBlanketAgreementId,
                        principalTable: "KABA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ABA1_KARI_ARInvoiceId",
                        column: x => x.ARInvoiceId,
                        principalTable: "KARI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ABA1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ARI1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ARInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ARI1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARI1_KARI_ARInvoiceId",
                        column: x => x.ARInvoiceId,
                        principalTable: "KARI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ARI1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ARI2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ARInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_ARI2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARI2_KARI_ARInvoiceId",
                        column: x => x.ARInvoiceId,
                        principalTable: "KARI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ARR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnRequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ARR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARR1_KARR_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalTable: "KARR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ARR1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ARV1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ARReserveInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ARV1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARV1_KARV_ARReserveInvoiceId",
                        column: x => x.ARReserveInvoiceId,
                        principalTable: "KARV",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ARV1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ASO1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SaleOrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ASO1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASO1_KASO_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "KASO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASO1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ASQ1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SaleQuotionId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VatSum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Dimension1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension4 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Dimension5 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FreeText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ASQ1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASQ1_KASQ_SaleQuotionId",
                        column: x => x.SaleQuotionId,
                        principalTable: "KASQ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASQ1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BPR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BPReconciliationId = table.Column<int>(type: "integer", nullable: false),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    TransDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DocEntry = table.Column<int>(type: "integer", nullable: true),
                    DocNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    ReconciledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReconciledRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_BPR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BPR1_KBPR_BPReconciliationId",
                        column: x => x.BPReconciliationId,
                        principalTable: "KBPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LDC2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LandedCostId = table.Column<int>(type: "integer", nullable: false),
                    CostType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_LDC2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LDC2_KLDC_LandedCostId",
                        column: x => x.LandedCostId,
                        principalTable: "KLDC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAC1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APCreditNoteId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PAC1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAC1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAC1_KPAC_APCreditNoteId",
                        column: x => x.APCreditNoteId,
                        principalTable: "KPAC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAI1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PAI1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAI1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAI1_KPAI_APInvoiceId",
                        column: x => x.APInvoiceId,
                        principalTable: "KPAI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    APReserveInvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PAR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAR1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAR1_KPAI_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPAI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PAR1_KPAR_APReserveInvoiceId",
                        column: x => x.APReserveInvoiceId,
                        principalTable: "KPAR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PBA1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseBlanketAgreementId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PBA1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PBA1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PBA1_KPBA_PurchaseBlanketAgreementId",
                        column: x => x.PurchaseBlanketAgreementId,
                        principalTable: "KPBA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LDC1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LandedCostId = table.Column<int>(type: "integer", nullable: false),
                    GoodsReceiptPOId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_LDC1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LDC1_KLDC_LandedCostId",
                        column: x => x.LandedCostId,
                        principalTable: "KLDC",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LDC1_KPGR_GoodsReceiptPOId",
                        column: x => x.GoodsReceiptPOId,
                        principalTable: "KPGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PGQ1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoodsReturnRequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PGQ1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PGQ1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PGQ1_KPGQ_GoodsReturnRequestId",
                        column: x => x.GoodsReturnRequestId,
                        principalTable: "KPGQ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PGQ1_KPGR_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PGR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LineNum = table.Column<int>(type: "integer", nullable: false),
                    GoodsReceiptPOId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "text", nullable: true),
                    ItemName = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseLine = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PGR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PGR1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PGR1_KPGR_GoodsReceiptPOId",
                        column: x => x.GoodsReceiptPOId,
                        principalTable: "KPGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PGT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoodsReturnId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhsCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PGT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PGT1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PGT1_KPGR_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPGR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PGT1_KPGT_GoodsReturnId",
                        column: x => x.GoodsReturnId,
                        principalTable: "KPGT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPDR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KPDR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPDR_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KPDR_KPPO_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PPO1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PPO1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPO1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPO1_KPPO_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "KPPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PPO1_KPPQ_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPPQ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PPQ1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseQuotationId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PPQ1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPQ1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPQ1_KPBA_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPBA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PPQ1_KPPQ_PurchaseQuotationId",
                        column: x => x.PurchaseQuotationId,
                        principalTable: "KPPQ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PPR1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PPR1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPR1_KITM_ItemId",
                        column: x => x.ItemId,
                        principalTable: "KITM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPR1_KPPR_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "KPPR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APT1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovalTemplateId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_APT1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_APT1_KAPT_ApprovalTemplateId",
                        column: x => x.ApprovalTemplateId,
                        principalTable: "KAPT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_APT1_KUSR_UserId",
                        column: x => x.UserId,
                        principalTable: "KUSR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KALR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientUserId = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ReferenceKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_KALR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KALR_KUSR_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "KUSR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KALR_KUSR_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "KUSR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "KAPR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalTemplateId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedBy = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedBy = table.Column<int>(type: "integer", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_KAPR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KAPR_KAPT_ApprovalTemplateId",
                        column: x => x.ApprovalTemplateId,
                        principalTable: "KAPT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KAPR_KUSR_DecidedBy",
                        column: x => x.DecidedBy,
                        principalTable: "KUSR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KAPR_KUSR_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "KUSR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KPDI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocNum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    BaseEntry = table.Column<int>(type: "integer", nullable: true),
                    BaseType = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_KPDI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPDI_KOCR_VendorId",
                        column: x => x.VendorId,
                        principalTable: "KOCR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KPDI_KPDR_BaseEntry",
                        column: x => x.BaseEntry,
                        principalTable: "KPDR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ABA1_ARInvoiceId",
                table: "ABA1",
                column: "ARInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ABA1_ItemId",
                table: "ABA1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ABA1_SaleBlanketAgreementId",
                table: "ABA1",
                column: "SaleBlanketAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_ACN1_ARCreditNoteId",
                table: "ACN1",
                column: "ARCreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ACN1_ItemId",
                table: "ACN1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ADI1_ARDownPaymentInvoiceId",
                table: "ADI1",
                column: "ARDownPaymentInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ADI1_ItemId",
                table: "ADI1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ADR1_ArDownPaymentRequestId",
                table: "ADR1",
                column: "ArDownPaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ADR1_ItemId",
                table: "ADR1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ADV1_DeliveryId",
                table: "ADV1",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_ADV1_ItemId",
                table: "ADV1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_APT1_ApprovalTemplateId",
                table: "APT1",
                column: "ApprovalTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_APT1_UserId",
                table: "APT1",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ARE1_ItemId",
                table: "ARE1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ARE1_ReturnId",
                table: "ARE1",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ARI1_ARInvoiceId",
                table: "ARI1",
                column: "ARInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ARI1_ItemId",
                table: "ARI1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ARI2_ARInvoiceId",
                table: "ARI2",
                column: "ARInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ARR1_ItemId",
                table: "ARR1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ARR1_ReturnRequestId",
                table: "ARR1",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ARV1_ARReserveInvoiceId",
                table: "ARV1",
                column: "ARReserveInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ARV1_ItemId",
                table: "ARV1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ASO1_ItemId",
                table: "ASO1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ASO1_SaleOrderId",
                table: "ASO1",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ASQ1_ItemId",
                table: "ASQ1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ASQ1_SaleQuotionId",
                table: "ASQ1",
                column: "SaleQuotionId");

            migrationBuilder.CreateIndex(
                name: "IX_BCD1_BarCodeId",
                table: "BCD1",
                column: "BarCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BCD1_Code",
                table: "BCD1",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_BCD1_IsDefault",
                table: "BCD1",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_BGT1_AcctCode",
                table: "BGT1",
                column: "AcctCode");

            migrationBuilder.CreateIndex(
                name: "IX_BGT1_BudgetId",
                table: "BGT1",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BNR1_BankReconciliationId",
                table: "BNR1",
                column: "BankReconciliationId");

            migrationBuilder.CreateIndex(
                name: "IX_BNR1_DocType_DocEntry",
                table: "BNR1",
                columns: new[] { "DocType", "DocEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_BNR1_IsReconciled",
                table: "BNR1",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_BOM1_BillOfMaterialId",
                table: "BOM1",
                column: "BillOfMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BPR1_BPReconciliationId",
                table: "BPR1",
                column: "BPReconciliationId");

            migrationBuilder.CreateIndex(
                name: "IX_BPR1_DocType_DocEntry",
                table: "BPR1",
                columns: new[] { "DocType", "DocEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_BPR1_IsReconciled",
                table: "BPR1",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_CDF1_ConversionDifferenceId",
                table: "CDF1",
                column: "ConversionDifferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CMP1_CampaignId_BPCode",
                table: "CMP1",
                columns: new[] { "CampaignId", "BPCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_COE1_AcctCode",
                table: "COE1",
                column: "AcctCode");

            migrationBuilder.CreateIndex(
                name: "IX_COE1_BatchId",
                table: "COE1",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DEP1_DepositId",
                table: "DEP1",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_DEP1_IncomingPaymentId",
                table: "DEP1",
                column: "IncomingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_DNT1_DunningTermId_Level",
                table: "DNT1",
                columns: new[] { "DunningTermId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DPR1_AssetId",
                table: "DPR1",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DPR1_DepreciationRunId",
                table: "DPR1",
                column: "DepreciationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_DRL1_DistributionRuleId",
                table: "DRL1",
                column: "DistributionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DUN1_ARInvoiceId",
                table: "DUN1",
                column: "ARInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DUN1_DunningLetterId",
                table: "DUN1",
                column: "DunningLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_ERD1_ExchangeRateDifferenceId",
                table: "ERD1",
                column: "ExchangeRateDifferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FRT1_TemplateId",
                table: "FRT1",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FRT1_TemplateId_RowCode",
                table: "FRT1",
                columns: new[] { "TemplateId", "RowCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GIE1_GoodsIssueId",
                table: "GIE1",
                column: "GoodsIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_GRT1_GoodsReceiptId",
                table: "GRT1",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_JRN1_AccountCode",
                table: "JRN1",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_JRN1_CardCode",
                table: "JRN1",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_JRN1_JournalEntryId",
                table: "JRN1",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JVH1_AccountCode",
                table: "JVH1",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_JVH1_JournalVoucherId",
                table: "JVH1",
                column: "JournalVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_KABA_CustomerId",
                table: "KABA",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KACA_ActivityId",
                table: "KACA",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_KACN_CustomerId",
                table: "KACN",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KACT_ActivityNo",
                table: "KACT",
                column: "ActivityNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KAD1_AddressFormatId",
                table: "KAD1",
                column: "AddressFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_KADF_Code",
                table: "KADF",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KADI_BaseEntry",
                table: "KADI",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_KADI_CustomerId",
                table: "KADI",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KADR_CustomerId",
                table: "KADR",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KADV_CustomerId",
                table: "KADV",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KALR_RecipientUserId_IsRead",
                table: "KALR",
                columns: new[] { "RecipientUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_KALR_RecipientUserId_ReferenceKey",
                table: "KALR",
                columns: new[] { "RecipientUserId", "ReferenceKey" });

            migrationBuilder.CreateIndex(
                name: "IX_KALR_SenderUserId",
                table: "KALR",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KAPR_ApprovalTemplateId",
                table: "KAPR",
                column: "ApprovalTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_KAPR_DecidedBy",
                table: "KAPR",
                column: "DecidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KAPR_DocumentType_DocumentId_Status",
                table: "KAPR",
                columns: new[] { "DocumentType", "DocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_KAPR_RequestedBy",
                table: "KAPR",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KARE_CustomerId",
                table: "KARE",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KARI_CustomerId",
                table: "KARI",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KARR_CustomerId",
                table: "KARR",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KARV_CustomerId",
                table: "KARV",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KASO_CustomerId",
                table: "KASO",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KASQ_CustomerId",
                table: "KASQ",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_KAST_ItemNo",
                table: "KAST",
                column: "ItemNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KATF_OwnerType_OwnerId",
                table: "KATF",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_KATT_EmployeeCode_Date",
                table: "KATT",
                columns: new[] { "EmployeeCode", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBAT_ExpiryDate",
                table: "KBAT",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_KBAT_ItemCode_WhsCode_BatchNo",
                table: "KBAT",
                columns: new[] { "ItemCode", "WhsCode", "BatchNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBAT_Serial",
                table: "KBAT",
                columns: new[] { "ItemCode", "BatchNo" },
                unique: true,
                filter: "\"Kind\" = 'Serial'");

            migrationBuilder.CreateIndex(
                name: "IX_KBCD_ItemNo",
                table: "KBCD",
                column: "ItemNo");

            migrationBuilder.CreateIndex(
                name: "IX_KBCD_ItemNo_BarCodeType",
                table: "KBCD",
                columns: new[] { "ItemNo", "BarCodeType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBGT_BudgetNo",
                table: "KBGT",
                column: "BudgetNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBGT_FiscalYear",
                table: "KBGT",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_KBGT_Status",
                table: "KBGT",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KBNR_AccountCode",
                table: "KBNR",
                column: "AccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_KBNR_ReconcNo",
                table: "KBNR",
                column: "ReconcNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBNR_StatementDate",
                table: "KBNR",
                column: "StatementDate");

            migrationBuilder.CreateIndex(
                name: "IX_KBNR_Status",
                table: "KBNR",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KBOM_Code",
                table: "KBOM",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBOM_ParentItemCode",
                table: "KBOM",
                column: "ParentItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_KBPP_Code",
                table: "KBPP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBPR_PartnerId",
                table: "KBPR",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_KBPR_ReconcNo",
                table: "KBPR",
                column: "ReconcNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBPR_Status",
                table: "KBPR",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KBRN_BranchCode",
                table: "KBRN",
                column: "BranchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KBTX_BaseDocType_BaseDocEntry",
                table: "KBTX",
                columns: new[] { "BaseDocType", "BaseDocEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_KBTX_ItemCode_BatchNo",
                table: "KBTX",
                columns: new[] { "ItemCode", "BatchNo" });

            migrationBuilder.CreateIndex(
                name: "IX_KCCD_Code",
                table: "KCCD",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCDF_DocNo",
                table: "KCDF",
                column: "DocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCDF_SystemCurrency_PostingDate",
                table: "KCDF",
                columns: new[] { "SystemCurrency", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_KCLY_ItemCode_WhsCode_ReceivedDate_Id",
                table: "KCLY",
                columns: new[] { "ItemCode", "WhsCode", "ReceivedDate", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_KCM1_CommissionGroupId",
                table: "KCM1",
                column: "CommissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KCMG_Code",
                table: "KCMG",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCMP_CampaignNo",
                table: "KCMP",
                column: "CampaignNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCMP_Status",
                table: "KCMP",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KCND_CandidateNo",
                table: "KCND",
                column: "CandidateNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCND_JobPostingId",
                table: "KCND",
                column: "JobPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_KCND_Stage",
                table: "KCND",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_KCOA_AcctCode",
                table: "KCOA",
                column: "AcctCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCOA_AcctType",
                table: "KCOA",
                column: "AcctType");

            migrationBuilder.CreateIndex(
                name: "IX_KCOA_FatherNum",
                table: "KCOA",
                column: "FatherNum");

            migrationBuilder.CreateIndex(
                name: "IX_KCOA_IsActive",
                table: "KCOA",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KCOA_Level",
                table: "KCOA",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_KCOE_DocNo",
                table: "KCOE",
                column: "DocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCOE_Status_BatchDate",
                table: "KCOE",
                columns: new[] { "Status", "BatchDate" });

            migrationBuilder.CreateIndex(
                name: "IX_KCPR_BPId",
                table: "KCPR",
                column: "BPId");

            migrationBuilder.CreateIndex(
                name: "IX_KCRB_BPId",
                table: "KCRB",
                column: "BPId");

            migrationBuilder.CreateIndex(
                name: "IX_KCRD_BPId",
                table: "KCRD",
                column: "BPId");

            migrationBuilder.CreateIndex(
                name: "IX_KCRP_ElementKey_DocumentType_Language",
                table: "KCRP",
                columns: new[] { "ElementKey", "DocumentType", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCST_Code",
                table: "KCST",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KCUR_Code",
                table: "KCUR",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDDS_DocumentType",
                table: "KDDS",
                column: "DocumentType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDEP_DocNum",
                table: "KDEP",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDIE_CreatedAt",
                table: "KDIE",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KDIE_Direction_EntityKey",
                table: "KDIE",
                columns: new[] { "Direction", "EntityKey" });

            migrationBuilder.CreateIndex(
                name: "IX_KDIE_JobNo",
                table: "KDIE",
                column: "JobNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDNR_DocumentType",
                table: "KDNR",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_KDNR_DocumentType_SeriesName",
                table: "KDNR",
                columns: new[] { "DocumentType", "SeriesName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDNR_IsDefault",
                table: "KDNR",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_KDNT_Code",
                table: "KDNT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDPR_PeriodYear_PeriodMonth",
                table: "KDPR",
                columns: new[] { "PeriodYear", "PeriodMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_KDPR_RunNo",
                table: "KDPR",
                column: "RunNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDRL_RuleCode",
                table: "KDRL",
                column: "RuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDS1_DashboardConfigId",
                table: "KDS1",
                column: "DashboardConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_KDSP_Code",
                table: "KDSP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDUN_CustomerCode",
                table: "KDUN",
                column: "CustomerCode");

            migrationBuilder.CreateIndex(
                name: "IX_KDUN_LetterNo",
                table: "KDUN",
                column: "LetterNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KDUN_RunDate",
                table: "KDUN",
                column: "RunDate");

            migrationBuilder.CreateIndex(
                name: "IX_KECA_ElectronicCertificateId",
                table: "KECA",
                column: "ElectronicCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_KECT_CertNo",
                table: "KECT",
                column: "CertNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KED1_ElectronicDocumentId",
                table: "KED1",
                column: "ElectronicDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_KEDM_DocumentNo",
                table: "KEDM",
                column: "DocumentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KEFM_FileCode",
                table: "KEFM",
                column: "FileCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KEFV_ElectronicFileId",
                table: "KEFV",
                column: "ElectronicFileId");

            migrationBuilder.CreateIndex(
                name: "IX_KEMP_Code",
                table: "KEMP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KERD_Currency_PostingDate",
                table: "KERD",
                columns: new[] { "Currency", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_KERD_DocNo",
                table: "KERD",
                column: "DocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KFRT_Code",
                table: "KFRT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KFRT_ReportType_IsDefault",
                table: "KFRT",
                columns: new[] { "ReportType", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_KGLO_Scope_GroupKey_Field",
                table: "KGLO",
                columns: new[] { "Scope", "GroupKey", "Field" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KINM_BaseDocType_BaseDocEntry",
                table: "KINM",
                columns: new[] { "BaseDocType", "BaseDocEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_KINM_ItemCode_WhsCode_PostingDate",
                table: "KINM",
                columns: new[] { "ItemCode", "WhsCode", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_KIPL_PriceListId_ItemCode",
                table: "KIPL",
                columns: new[] { "PriceListId", "ItemCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KITB_Code",
                table: "KITB",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KITG_Code",
                table: "KITG",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KITM_Barcode",
                table: "KITM",
                column: "Barcode",
                unique: true,
                filter: "\"Barcode\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_KITM_ItemCode",
                table: "KITM",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KJOB_PostingNo",
                table: "KJOB",
                column: "PostingNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KJOB_Status",
                table: "KJOB",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KJRN_BaseDocType_BaseDocEntry",
                table: "KJRN",
                columns: new[] { "BaseDocType", "BaseDocEntry" });

            migrationBuilder.CreateIndex(
                name: "IX_KJRN_JrnlNo",
                table: "KJRN",
                column: "JrnlNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KJRN_TransDate",
                table: "KJRN",
                column: "TransDate");

            migrationBuilder.CreateIndex(
                name: "IX_KJRN_TransType",
                table: "KJRN",
                column: "TransType");

            migrationBuilder.CreateIndex(
                name: "IX_KJVH_ApprovalStatus",
                table: "KJVH",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_KJVH_Status",
                table: "KJVH",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KJVH_VoucherDate",
                table: "KJVH",
                column: "VoucherDate");

            migrationBuilder.CreateIndex(
                name: "IX_KJVH_VoucherNo",
                table: "KJVH",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KLCP_Code",
                table: "KLCP",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_KLCP_IsActive",
                table: "KLCP",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KLCTYP_Name",
                table: "KLCTYP",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KLDC_VendorId",
                table: "KLDC",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KLOC_Code",
                table: "KLOC",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KLVR_EmployeeCode_FromDate",
                table: "KLVR",
                columns: new[] { "EmployeeCode", "FromDate" });

            migrationBuilder.CreateIndex(
                name: "IX_KLVR_RequestNo",
                table: "KLVR",
                column: "RequestNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KLWU_Code",
                table: "KLWU",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KMAL_Alias",
                table: "KMAL",
                column: "Alias");

            migrationBuilder.CreateIndex(
                name: "IX_KMRC_Code",
                table: "KMRC",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KMSP_UserCode_EventKey_Channel",
                table: "KMSP",
                columns: new[] { "UserCode", "EventKey", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KOCR_ActiveStatus",
                table: "KOCR",
                column: "ActiveStatus");

            migrationBuilder.CreateIndex(
                name: "IX_KOCR_CardName",
                table: "KOCR",
                column: "CardName");

            migrationBuilder.CreateIndex(
                name: "IX_KOCR_Code",
                table: "KOCR",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KOCR_TypeStatus",
                table: "KOCR",
                column: "TypeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_KOPA_OpportunityId",
                table: "KOPA",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KOPC_OpportunityId",
                table: "KOPC",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KOPI_OpportunityId",
                table: "KOPI",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KOPN_OpportunityId",
                table: "KOPN",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KOPP_OpportunityNo",
                table: "KOPP",
                column: "OpportunityNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KOPPCMP_Name",
                table: "KOPPCMP",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KOPPSTG_Name",
                table: "KOPPSTG",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KOPR_OpportunityId",
                table: "KOPR",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KOPS_OpportunityId",
                table: "KOPS",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_KPAC_VendorId",
                table: "KPAC",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPAI_VendorId",
                table: "KPAI",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPAR_VendorId",
                table: "KPAR",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPAY_EmployeeCode_PeriodYear_PeriodMonth",
                table: "KPAY",
                columns: new[] { "EmployeeCode", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPBA_VendorId",
                table: "KPBA",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPCK_PickNumber",
                table: "KPCK",
                column: "PickNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPCK_Status",
                table: "KPCK",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KPDI_BaseEntry",
                table: "KPDI",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_KPDI_VendorId",
                table: "KPDI",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPDR_BaseEntry",
                table: "KPDR",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_KPDR_VendorId",
                table: "KPDR",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPGQ_VendorId",
                table: "KPGQ",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPGR_DocNum",
                table: "KPGR",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPGR_VendorId",
                table: "KPGR",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPGT_VendorId",
                table: "KPGT",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPKT_Code",
                table: "KPKT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPMT_BPId",
                table: "KPMT",
                column: "BPId");

            migrationBuilder.CreateIndex(
                name: "IX_KPPO_DocNum",
                table: "KPPO",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPPO_VendorId",
                table: "KPPO",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPPQ_DocNum",
                table: "KPPQ",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPPQ_VendorId",
                table: "KPPQ",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPPR_DocNum",
                table: "KPPR",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPPR_VendorId",
                table: "KPPR",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_KPRC_CostCenterCode",
                table: "KPRC",
                column: "CostCenterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPRC_IsActive",
                table: "KPRC",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KPRC_ParentCode",
                table: "KPRC",
                column: "ParentCode");

            migrationBuilder.CreateIndex(
                name: "IX_KPRD_PeriodCode",
                table: "KPRD",
                column: "PeriodCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPRI_IndexCode_EffectiveDate",
                table: "KPRI",
                columns: new[] { "IndexCode", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPRJ_ProjectCode",
                table: "KPRJ",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPRL_PeriodYear_PeriodMonth_Trigger",
                table: "KPRL",
                columns: new[] { "PeriodYear", "PeriodMonth", "Trigger" });

            migrationBuilder.CreateIndex(
                name: "IX_KPRM_ProjectNo",
                table: "KPRM",
                column: "ProjectNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPRM_ProjectType",
                table: "KPRM",
                column: "ProjectType");

            migrationBuilder.CreateIndex(
                name: "IX_KPRM_Status",
                table: "KPRM",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KPRS_ProjectId_LineNo",
                table: "KPRS",
                columns: new[] { "ProjectId", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPTX_Code",
                table: "KPTX",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPYI_CardCode",
                table: "KPYI",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_KPYI_DocDate",
                table: "KPYI",
                column: "DocDate");

            migrationBuilder.CreateIndex(
                name: "IX_KPYI_DocNum",
                table: "KPYI",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPYI_Status",
                table: "KPYI",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KPYO_CardCode",
                table: "KPYO",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_KPYO_DocDate",
                table: "KPYO",
                column: "DocDate");

            migrationBuilder.CreateIndex(
                name: "IX_KPYO_DocNum",
                table: "KPYO",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPYO_Status",
                table: "KPYO",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KRFL_Code",
                table: "KRFL",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KRFL_SourceModule_SourceField",
                table: "KRFL",
                columns: new[] { "SourceModule", "SourceField" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KRLM_Code",
                table: "KRLM",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KRLP_ReportLayoutId",
                table: "KRLP",
                column: "ReportLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_KRODBU_Username",
                table: "KRODBU",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KRTT_Currency_EffectiveDate",
                table: "KRTT",
                columns: new[] { "Currency", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSCN_ContractNo",
                table: "KSCN",
                column: "ContractNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSCN_CustomerCode",
                table: "KSCN",
                column: "CustomerCode");

            migrationBuilder.CreateIndex(
                name: "IX_KSCN_Status",
                table: "KSCN",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KSEB_Code",
                table: "KSEB",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSHT_Code",
                table: "KSHT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSPC_PrinterCode",
                table: "KSPC",
                column: "PrinterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSPL_Code",
                table: "KSPL",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSPR_ServerPrintConfigId",
                table: "KSPR",
                column: "ServerPrintConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_KSPU_SAPLinkId",
                table: "KSPU",
                column: "SAPLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_KSRCH_Name",
                table: "KSRCH",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSRV_Number",
                table: "KSRV",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSRV_Status",
                table: "KSRV",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KSTC_Code",
                table: "KSTC",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSTO_CardCode",
                table: "KSTO",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_KSTO_DocNo",
                table: "KSTO",
                column: "DocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSTO_FromWarehouse_ToWarehouse",
                table: "KSTO",
                columns: new[] { "FromWarehouse", "ToWarehouse" });

            migrationBuilder.CreateIndex(
                name: "IX_KSTO_Number",
                table: "KSTO",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSTO_PostingDate",
                table: "KSTO",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_KSTR_CardCode",
                table: "KSTR",
                column: "CardCode");

            migrationBuilder.CreateIndex(
                name: "IX_KSTR_DocNo",
                table: "KSTR",
                column: "DocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSTR_FromWarehouse_ToWarehouse",
                table: "KSTR",
                columns: new[] { "FromWarehouse", "ToWarehouse" });

            migrationBuilder.CreateIndex(
                name: "IX_KSTR_PostingDate",
                table: "KSTR",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_KSTR_Status",
                table: "KSTR",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KSVC_CustomerCode",
                table: "KSVC",
                column: "CustomerCode");

            migrationBuilder.CreateIndex(
                name: "IX_KSVC_ServiceCallNo",
                table: "KSVC",
                column: "ServiceCallNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KSVC_Status",
                table: "KSVC",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KTAX_Code",
                table: "KTAX",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KTER_Code",
                table: "KTER",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KTER_ParentId",
                table: "KTER",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_KUAU_UserId",
                table: "KUAU",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUDF_UserCode",
                table: "KUDF",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUDT_Code",
                table: "KUDT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUDV_TableName_RecordId_FieldName",
                table: "KUDV",
                columns: new[] { "TableName", "RecordId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUG1_UserGroupId",
                table: "KUG1",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KUG2_UserGroupId",
                table: "KUG2",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KUGP_Code",
                table: "KUGP",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUGR_Code",
                table: "KUGR",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUOM_Code",
                table: "KUOM",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUSG_GroupName",
                table: "KUSG",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_KUSG_GroupType",
                table: "KUSG",
                column: "GroupType");

            migrationBuilder.CreateIndex(
                name: "IX_KUSR_Email",
                table: "KUSR",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUSR_UserGroupId",
                table: "KUSR",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_KUSR_UserId",
                table: "KUSR",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUTF_UserTableId_FieldCode",
                table: "KUTF",
                columns: new[] { "UserTableId", "FieldCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KUTR_UserTableId_RowKey",
                table: "KUTR",
                columns: new[] { "UserTableId", "RowKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KWHS_WhsCode",
                table: "KWHS",
                column: "WhsCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KWHT_Code",
                table: "KWHT",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KWOR_DocNum",
                table: "KWOR",
                column: "DocNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KWOR_Status",
                table: "KWOR",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KWTU_Code",
                table: "KWTU",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LDC1_GoodsReceiptPOId",
                table: "LDC1",
                column: "GoodsReceiptPOId");

            migrationBuilder.CreateIndex(
                name: "IX_LDC1_LandedCostId_GoodsReceiptPOId",
                table: "LDC1",
                columns: new[] { "LandedCostId", "GoodsReceiptPOId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LDC2_LandedCostId",
                table: "LDC2",
                column: "LandedCostId");

            migrationBuilder.CreateIndex(
                name: "IX_OPLN_BasePriceListId",
                table: "OPLN",
                column: "BasePriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_OPLN_Name",
                table: "OPLN",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OUDF_TableName_FieldName",
                table: "OUDF",
                columns: new[] { "TableName", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PAC1_APCreditNoteId",
                table: "PAC1",
                column: "APCreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PAC1_ItemId",
                table: "PAC1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PAI1_APInvoiceId",
                table: "PAI1",
                column: "APInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PAI1_ItemId",
                table: "PAI1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PAR1_APReserveInvoiceId",
                table: "PAR1",
                column: "APReserveInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PAR1_BaseEntry",
                table: "PAR1",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PAR1_ItemId",
                table: "PAR1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PBA1_ItemId",
                table: "PBA1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PBA1_PurchaseBlanketAgreementId",
                table: "PBA1",
                column: "PurchaseBlanketAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_PCK1_PickListId",
                table: "PCK1",
                column: "PickListId");

            migrationBuilder.CreateIndex(
                name: "IX_PGQ1_BaseEntry",
                table: "PGQ1",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PGQ1_GoodsReturnRequestId",
                table: "PGQ1",
                column: "GoodsReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PGQ1_ItemId",
                table: "PGQ1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PGR1_GoodsReceiptPOId_LineNum",
                table: "PGR1",
                columns: new[] { "GoodsReceiptPOId", "LineNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PGR1_ItemId",
                table: "PGR1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PGT1_BaseEntry",
                table: "PGT1",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PGT1_GoodsReturnId",
                table: "PGT1",
                column: "GoodsReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PGT1_ItemId",
                table: "PGT1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PKL1_PickPackCriteriaId",
                table: "PKL1",
                column: "PickPackCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PPO1_BaseEntry",
                table: "PPO1",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PPO1_ItemId",
                table: "PPO1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPO1_PurchaseOrderId",
                table: "PPO1",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PPQ1_BaseEntry",
                table: "PPQ1",
                column: "BaseEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PPQ1_ItemId",
                table: "PPQ1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPQ1_PurchaseQuotationId",
                table: "PPQ1",
                column: "PurchaseQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PPR1_ItemId",
                table: "PPR1",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPR1_PurchaseRequestId",
                table: "PPR1",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PYI1_IncomingPaymentId",
                table: "PYI1",
                column: "IncomingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PYI1_InvoiceId",
                table: "PYI1",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PYO1_InvoiceId",
                table: "PYO1",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PYO1_OutgoingPaymentId",
                table: "PYO1",
                column: "OutgoingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RCR1_RecurringPostingId",
                table: "RCR1",
                column: "RecurringPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_SRC1_SavedSearchId",
                table: "SRC1",
                column: "SavedSearchId");

            migrationBuilder.CreateIndex(
                name: "IX_SRV1_StockRevaluationId",
                table: "SRV1",
                column: "StockRevaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_SRV2_StockRevaluationId",
                table: "SRV2",
                column: "StockRevaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_STC1_StockCycleId_ItemCode_WhsCode",
                table: "STC1",
                columns: new[] { "StockCycleId", "ItemCode", "WhsCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_STO1_ItemNo",
                table: "STO1",
                column: "ItemNo");

            migrationBuilder.CreateIndex(
                name: "IX_STO1_StockTransferId",
                table: "STO1",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_STR1_ItemNo",
                table: "STR1",
                column: "ItemNo");

            migrationBuilder.CreateIndex(
                name: "IX_STR1_StockTransferRequestId",
                table: "STR1",
                column: "StockTransferRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_UGP1_UomGroupId",
                table: "UGP1",
                column: "UomGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WOR1_ProductionOrderId",
                table: "WOR1",
                column: "ProductionOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ABA1");

            migrationBuilder.DropTable(
                name: "ACN1");

            migrationBuilder.DropTable(
                name: "ADI1");

            migrationBuilder.DropTable(
                name: "ADR1");

            migrationBuilder.DropTable(
                name: "ADV1");

            migrationBuilder.DropTable(
                name: "APT1");

            migrationBuilder.DropTable(
                name: "ARE1");

            migrationBuilder.DropTable(
                name: "ARI1");

            migrationBuilder.DropTable(
                name: "ARI2");

            migrationBuilder.DropTable(
                name: "ARR1");

            migrationBuilder.DropTable(
                name: "ARV1");

            migrationBuilder.DropTable(
                name: "ASO1");

            migrationBuilder.DropTable(
                name: "ASQ1");

            migrationBuilder.DropTable(
                name: "BCD1");

            migrationBuilder.DropTable(
                name: "BGT1");

            migrationBuilder.DropTable(
                name: "BNR1");

            migrationBuilder.DropTable(
                name: "BOM1");

            migrationBuilder.DropTable(
                name: "BPR1");

            migrationBuilder.DropTable(
                name: "CDF1");

            migrationBuilder.DropTable(
                name: "CMP1");

            migrationBuilder.DropTable(
                name: "COE1");

            migrationBuilder.DropTable(
                name: "DEP1");

            migrationBuilder.DropTable(
                name: "DNT1");

            migrationBuilder.DropTable(
                name: "DPR1");

            migrationBuilder.DropTable(
                name: "DRL1");

            migrationBuilder.DropTable(
                name: "DUN1");

            migrationBuilder.DropTable(
                name: "ERD1");

            migrationBuilder.DropTable(
                name: "FRT1");

            migrationBuilder.DropTable(
                name: "GIE1");

            migrationBuilder.DropTable(
                name: "GRT1");

            migrationBuilder.DropTable(
                name: "JRN1");

            migrationBuilder.DropTable(
                name: "JVH1");

            migrationBuilder.DropTable(
                name: "KACA");

            migrationBuilder.DropTable(
                name: "KAD1");

            migrationBuilder.DropTable(
                name: "KAFF");

            migrationBuilder.DropTable(
                name: "KALR");

            migrationBuilder.DropTable(
                name: "KAPR");

            migrationBuilder.DropTable(
                name: "KAST");

            migrationBuilder.DropTable(
                name: "KATF");

            migrationBuilder.DropTable(
                name: "KATT");

            migrationBuilder.DropTable(
                name: "KBAT");

            migrationBuilder.DropTable(
                name: "KBPP");

            migrationBuilder.DropTable(
                name: "KBRN");

            migrationBuilder.DropTable(
                name: "KBTX");

            migrationBuilder.DropTable(
                name: "KCCD");

            migrationBuilder.DropTable(
                name: "KCLY");

            migrationBuilder.DropTable(
                name: "KCM1");

            migrationBuilder.DropTable(
                name: "KCND");

            migrationBuilder.DropTable(
                name: "KCOA");

            migrationBuilder.DropTable(
                name: "KCOM");

            migrationBuilder.DropTable(
                name: "KCPR");

            migrationBuilder.DropTable(
                name: "KCRB");

            migrationBuilder.DropTable(
                name: "KCRD");

            migrationBuilder.DropTable(
                name: "KCRP");

            migrationBuilder.DropTable(
                name: "KCRS");

            migrationBuilder.DropTable(
                name: "KCST");

            migrationBuilder.DropTable(
                name: "KCUR");

            migrationBuilder.DropTable(
                name: "KDDS");

            migrationBuilder.DropTable(
                name: "KDGS");

            migrationBuilder.DropTable(
                name: "KDIE");

            migrationBuilder.DropTable(
                name: "KDNR");

            migrationBuilder.DropTable(
                name: "KDS1");

            migrationBuilder.DropTable(
                name: "KDSP");

            migrationBuilder.DropTable(
                name: "KECA");

            migrationBuilder.DropTable(
                name: "KED1");

            migrationBuilder.DropTable(
                name: "KEFV");

            migrationBuilder.DropTable(
                name: "KEML");

            migrationBuilder.DropTable(
                name: "KEMP");

            migrationBuilder.DropTable(
                name: "KFXS");

            migrationBuilder.DropTable(
                name: "KGLD");

            migrationBuilder.DropTable(
                name: "KGLO");

            migrationBuilder.DropTable(
                name: "KGST");

            migrationBuilder.DropTable(
                name: "KIMP");

            migrationBuilder.DropTable(
                name: "KINM");

            migrationBuilder.DropTable(
                name: "KIPL");

            migrationBuilder.DropTable(
                name: "KITB");

            migrationBuilder.DropTable(
                name: "KITG");

            migrationBuilder.DropTable(
                name: "KIVY");

            migrationBuilder.DropTable(
                name: "KJOB");

            migrationBuilder.DropTable(
                name: "KLCP");

            migrationBuilder.DropTable(
                name: "KLCTYP");

            migrationBuilder.DropTable(
                name: "KLOC");

            migrationBuilder.DropTable(
                name: "KLVR");

            migrationBuilder.DropTable(
                name: "KLWU");

            migrationBuilder.DropTable(
                name: "KMAL");

            migrationBuilder.DropTable(
                name: "KMRC");

            migrationBuilder.DropTable(
                name: "KMSP");

            migrationBuilder.DropTable(
                name: "KOPA");

            migrationBuilder.DropTable(
                name: "KOPC");

            migrationBuilder.DropTable(
                name: "KOPI");

            migrationBuilder.DropTable(
                name: "KOPN");

            migrationBuilder.DropTable(
                name: "KOPPCMP");

            migrationBuilder.DropTable(
                name: "KOPPRSN");

            migrationBuilder.DropTable(
                name: "KOPPSTG");

            migrationBuilder.DropTable(
                name: "KOPR");

            migrationBuilder.DropTable(
                name: "KOPS");

            migrationBuilder.DropTable(
                name: "KPAY");

            migrationBuilder.DropTable(
                name: "KPDI");

            migrationBuilder.DropTable(
                name: "KPKT");

            migrationBuilder.DropTable(
                name: "KPMT");

            migrationBuilder.DropTable(
                name: "KPRC");

            migrationBuilder.DropTable(
                name: "KPRD");

            migrationBuilder.DropTable(
                name: "KPRDS");

            migrationBuilder.DropTable(
                name: "KPRI");

            migrationBuilder.DropTable(
                name: "KPRJ");

            migrationBuilder.DropTable(
                name: "KPRL");

            migrationBuilder.DropTable(
                name: "KPRS");

            migrationBuilder.DropTable(
                name: "KPTX");

            migrationBuilder.DropTable(
                name: "KPWDPOL");

            migrationBuilder.DropTable(
                name: "KRFL");

            migrationBuilder.DropTable(
                name: "KRLP");

            migrationBuilder.DropTable(
                name: "KRODBU");

            migrationBuilder.DropTable(
                name: "KRTT");

            migrationBuilder.DropTable(
                name: "KSCN");

            migrationBuilder.DropTable(
                name: "KSEB");

            migrationBuilder.DropTable(
                name: "KSHT");

            migrationBuilder.DropTable(
                name: "KSPR");

            migrationBuilder.DropTable(
                name: "KSPU");

            migrationBuilder.DropTable(
                name: "KSVC");

            migrationBuilder.DropTable(
                name: "KTAX");

            migrationBuilder.DropTable(
                name: "KTER");

            migrationBuilder.DropTable(
                name: "KTXD");

            migrationBuilder.DropTable(
                name: "KUAU");

            migrationBuilder.DropTable(
                name: "KUDF");

            migrationBuilder.DropTable(
                name: "KUDV");

            migrationBuilder.DropTable(
                name: "KUG1");

            migrationBuilder.DropTable(
                name: "KUG2");

            migrationBuilder.DropTable(
                name: "KUOM");

            migrationBuilder.DropTable(
                name: "KUTF");

            migrationBuilder.DropTable(
                name: "KUTR");

            migrationBuilder.DropTable(
                name: "KWHS");

            migrationBuilder.DropTable(
                name: "KWHT");

            migrationBuilder.DropTable(
                name: "KWTU");

            migrationBuilder.DropTable(
                name: "LDC1");

            migrationBuilder.DropTable(
                name: "LDC2");

            migrationBuilder.DropTable(
                name: "OUDF");

            migrationBuilder.DropTable(
                name: "PAC1");

            migrationBuilder.DropTable(
                name: "PAI1");

            migrationBuilder.DropTable(
                name: "PAR1");

            migrationBuilder.DropTable(
                name: "PBA1");

            migrationBuilder.DropTable(
                name: "PCK1");

            migrationBuilder.DropTable(
                name: "PGQ1");

            migrationBuilder.DropTable(
                name: "PGR1");

            migrationBuilder.DropTable(
                name: "PGT1");

            migrationBuilder.DropTable(
                name: "PKL1");

            migrationBuilder.DropTable(
                name: "PPO1");

            migrationBuilder.DropTable(
                name: "PPQ1");

            migrationBuilder.DropTable(
                name: "PPR1");

            migrationBuilder.DropTable(
                name: "PYI1");

            migrationBuilder.DropTable(
                name: "PYO1");

            migrationBuilder.DropTable(
                name: "RCR1");

            migrationBuilder.DropTable(
                name: "SRC1");

            migrationBuilder.DropTable(
                name: "SRV1");

            migrationBuilder.DropTable(
                name: "SRV2");

            migrationBuilder.DropTable(
                name: "STC1");

            migrationBuilder.DropTable(
                name: "STO1");

            migrationBuilder.DropTable(
                name: "STR1");

            migrationBuilder.DropTable(
                name: "UGP1");

            migrationBuilder.DropTable(
                name: "WOR1");

            migrationBuilder.DropTable(
                name: "KABA");

            migrationBuilder.DropTable(
                name: "KACN");

            migrationBuilder.DropTable(
                name: "KADI");

            migrationBuilder.DropTable(
                name: "KADR");

            migrationBuilder.DropTable(
                name: "KADV");

            migrationBuilder.DropTable(
                name: "KARE");

            migrationBuilder.DropTable(
                name: "KARI");

            migrationBuilder.DropTable(
                name: "KARR");

            migrationBuilder.DropTable(
                name: "KARV");

            migrationBuilder.DropTable(
                name: "KASO");

            migrationBuilder.DropTable(
                name: "KASQ");

            migrationBuilder.DropTable(
                name: "KBCD");

            migrationBuilder.DropTable(
                name: "KBGT");

            migrationBuilder.DropTable(
                name: "KBNR");

            migrationBuilder.DropTable(
                name: "KBOM");

            migrationBuilder.DropTable(
                name: "KBPR");

            migrationBuilder.DropTable(
                name: "KCDF");

            migrationBuilder.DropTable(
                name: "KCMP");

            migrationBuilder.DropTable(
                name: "KCOE");

            migrationBuilder.DropTable(
                name: "KDEP");

            migrationBuilder.DropTable(
                name: "KDNT");

            migrationBuilder.DropTable(
                name: "KDPR");

            migrationBuilder.DropTable(
                name: "KDRL");

            migrationBuilder.DropTable(
                name: "KDUN");

            migrationBuilder.DropTable(
                name: "KERD");

            migrationBuilder.DropTable(
                name: "KFRT");

            migrationBuilder.DropTable(
                name: "KGIE");

            migrationBuilder.DropTable(
                name: "KGRT");

            migrationBuilder.DropTable(
                name: "KJRN");

            migrationBuilder.DropTable(
                name: "KJVH");

            migrationBuilder.DropTable(
                name: "KACT");

            migrationBuilder.DropTable(
                name: "KADF");

            migrationBuilder.DropTable(
                name: "KAPT");

            migrationBuilder.DropTable(
                name: "KUSR");

            migrationBuilder.DropTable(
                name: "KCMG");

            migrationBuilder.DropTable(
                name: "KDSH");

            migrationBuilder.DropTable(
                name: "KECT");

            migrationBuilder.DropTable(
                name: "KEDM");

            migrationBuilder.DropTable(
                name: "KEFM");

            migrationBuilder.DropTable(
                name: "OPLN");

            migrationBuilder.DropTable(
                name: "KOPP");

            migrationBuilder.DropTable(
                name: "KPDR");

            migrationBuilder.DropTable(
                name: "KPRM");

            migrationBuilder.DropTable(
                name: "KRLM");

            migrationBuilder.DropTable(
                name: "KSPC");

            migrationBuilder.DropTable(
                name: "KSPL");

            migrationBuilder.DropTable(
                name: "KUGR");

            migrationBuilder.DropTable(
                name: "KUDT");

            migrationBuilder.DropTable(
                name: "KLDC");

            migrationBuilder.DropTable(
                name: "KPAC");

            migrationBuilder.DropTable(
                name: "KPAI");

            migrationBuilder.DropTable(
                name: "KPAR");

            migrationBuilder.DropTable(
                name: "KPCK");

            migrationBuilder.DropTable(
                name: "KPGQ");

            migrationBuilder.DropTable(
                name: "KPGR");

            migrationBuilder.DropTable(
                name: "KPGT");

            migrationBuilder.DropTable(
                name: "OPKL");

            migrationBuilder.DropTable(
                name: "KPBA");

            migrationBuilder.DropTable(
                name: "KPPQ");

            migrationBuilder.DropTable(
                name: "KITM");

            migrationBuilder.DropTable(
                name: "KPPR");

            migrationBuilder.DropTable(
                name: "KPYI");

            migrationBuilder.DropTable(
                name: "KPYO");

            migrationBuilder.DropTable(
                name: "ORCR");

            migrationBuilder.DropTable(
                name: "KSRCH");

            migrationBuilder.DropTable(
                name: "KSRV");

            migrationBuilder.DropTable(
                name: "KSTC");

            migrationBuilder.DropTable(
                name: "KSTO");

            migrationBuilder.DropTable(
                name: "KSTR");

            migrationBuilder.DropTable(
                name: "KUGP");

            migrationBuilder.DropTable(
                name: "KWOR");

            migrationBuilder.DropTable(
                name: "KUSG");

            migrationBuilder.DropTable(
                name: "KPPO");

            migrationBuilder.DropTable(
                name: "KOCR");
        }
    }
}
