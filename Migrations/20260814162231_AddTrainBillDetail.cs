using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainBillDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainBillDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrainName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RackNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerMobileNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendorMobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationOfPurchase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalInvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainBillDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainBillDetails_BillNumber",
                table: "TrainBillDetails",
                column: "BillNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrainBillDetails_ManagerMobileNo",
                table: "TrainBillDetails",
                column: "ManagerMobileNo");

            migrationBuilder.CreateIndex(
                name: "IX_TrainBillDetails_TrainNumber",
                table: "TrainBillDetails",
                column: "TrainNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainBillDetails");
        }
    }
}
