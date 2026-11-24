using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhoyaLedger.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    OpeningReceivable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farmers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FarmerCode = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Village = table.Column<string>(type: "TEXT", nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MilkType = table.Column<int>(type: "INTEGER", nullable: false),
                    AdvanceBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MilkPayable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MilkRateSlabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MilkType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFlat = table.Column<bool>(type: "INTEGER", nullable: false),
                    FlatRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MinFat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxFat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkRateSlabs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilkRateSlabs_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BatchDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", nullable: false),
                    MilkUsedLitres = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KhoyaProducedKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WoodCostRs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LabourCostRs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportCostRs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCostRs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostRs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPerKg = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    YieldPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionBatches_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShiftTallies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TallyDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Shift = table.Column<int>(type: "INTEGER", nullable: false),
                    MeasuredLitres = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SumFromEntries = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Variance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    EnteredByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTallies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftTallies_AspNetUsers_EnteredByUserId",
                        column: x => x.EnteredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmerSettlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FarmerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodTo = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    GrossMilkValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdvanceRecovered = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPayable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingPayable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmerSettlements_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FarmerSettlements_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MilkCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FarmerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CollectionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Shift = table.Column<int>(type: "INTEGER", nullable: false),
                    Litres = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SnfPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MeasuredWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AppliedRateId = table.Column<int>(type: "INTEGER", nullable: true),
                    AppliedRateSlabId = table.Column<int>(type: "INTEGER", nullable: true),
                    AppliedRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GrossMilkValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CollectedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalCollectionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilkCollections_AspNetUsers_CollectedByUserId",
                        column: x => x.CollectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilkCollections_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilkCollections_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MilkCollections_MilkCollections_OriginalCollectionId",
                        column: x => x.OriginalCollectionId,
                        principalTable: "MilkCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MilkCollections_MilkRateSlabs_AppliedRateSlabId",
                        column: x => x.AppliedRateSlabId,
                        principalTable: "MilkRateSlabs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    QuantityKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DeliveryStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sales_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sales_ProductionBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Advances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FarmerId = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    RecoveryType = table.Column<int>(type: "INTEGER", nullable: false),
                    RecoveryValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RelatedSettlementId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advances_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Advances_FarmerSettlements_RelatedSettlementId",
                        column: x => x.RelatedSettlementId,
                        principalTable: "FarmerSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advances_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettlementId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaidDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementPayments_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementPayments_FarmerSettlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "FarmerSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchMilkLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LitresUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchMilkLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchMilkLinks_MilkCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "MilkCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchMilkLinks_ProductionBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettlementId = table.Column<int>(type: "INTEGER", nullable: false),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementCollections_FarmerSettlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "FarmerSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementCollections_MilkCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "MilkCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhoyaStockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovementDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: true),
                    SaleId = table.Column<int>(type: "INTEGER", nullable: true),
                    QuantityKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoyaStockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhoyaStockMovements_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KhoyaStockMovements_ProductionBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KhoyaStockMovements_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaidDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PostedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalePayments_AspNetUsers_PostedByUserId",
                        column: x => x.PostedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalePayments_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advances_FarmerId",
                table: "Advances",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_Advances_PostedByUserId",
                table: "Advances",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Advances_RelatedSettlementId",
                table: "Advances",
                column: "RelatedSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchMilkLinks_BatchId",
                table: "BatchMilkLinks",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchMilkLinks_CollectionId",
                table: "BatchMilkLinks",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_FarmerCode",
                table: "Farmers",
                column: "FarmerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSettlements_FarmerId",
                table: "FarmerSettlements",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSettlements_PostedByUserId",
                table: "FarmerSettlements",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoyaStockMovements_BatchId",
                table: "KhoyaStockMovements",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoyaStockMovements_PostedByUserId",
                table: "KhoyaStockMovements",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoyaStockMovements_SaleId",
                table: "KhoyaStockMovements",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_AppliedRateSlabId",
                table: "MilkCollections",
                column: "AppliedRateSlabId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_CollectedByUserId",
                table: "MilkCollections",
                column: "CollectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_FarmerId",
                table: "MilkCollections",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_OriginalCollectionId",
                table: "MilkCollections",
                column: "OriginalCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_PostedByUserId",
                table: "MilkCollections",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkCollections_ReceiptNumber",
                table: "MilkCollections",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MilkRateSlabs_CreatedByUserId",
                table: "MilkRateSlabs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_BatchNumber",
                table: "ProductionBatches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionBatches_PostedByUserId",
                table: "ProductionBatches",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_PostedByUserId",
                table: "SalePayments",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalePayments_SaleId",
                table: "SalePayments",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BatchId",
                table: "Sales",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_InvoiceNumber",
                table: "Sales",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_PostedByUserId",
                table: "Sales",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCollections_CollectionId",
                table: "SettlementCollections",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCollections_SettlementId",
                table: "SettlementCollections",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementPayments_PostedByUserId",
                table: "SettlementPayments",
                column: "PostedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementPayments_SettlementId",
                table: "SettlementPayments",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTallies_EnteredByUserId",
                table: "ShiftTallies",
                column: "EnteredByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advances");

            migrationBuilder.DropTable(
                name: "BatchMilkLinks");

            migrationBuilder.DropTable(
                name: "KhoyaStockMovements");

            migrationBuilder.DropTable(
                name: "SalePayments");

            migrationBuilder.DropTable(
                name: "SettlementCollections");

            migrationBuilder.DropTable(
                name: "SettlementPayments");

            migrationBuilder.DropTable(
                name: "ShiftTallies");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "MilkCollections");

            migrationBuilder.DropTable(
                name: "FarmerSettlements");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ProductionBatches");

            migrationBuilder.DropTable(
                name: "MilkRateSlabs");

            migrationBuilder.DropTable(
                name: "Farmers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
