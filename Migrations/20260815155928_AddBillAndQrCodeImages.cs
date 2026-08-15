using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBillAndQrCodeImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillImageContentType",
                table: "TrainBillDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BillImageData",
                table: "TrainBillDetails",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillImageUrl",
                table: "TrainBillDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImageContentType",
                table: "TrainBillDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "QrCodeImageData",
                table: "TrainBillDetails",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImageUrl",
                table: "TrainBillDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillImageContentType",
                table: "TrainBillDetails");

            migrationBuilder.DropColumn(
                name: "BillImageData",
                table: "TrainBillDetails");

            migrationBuilder.DropColumn(
                name: "BillImageUrl",
                table: "TrainBillDetails");

            migrationBuilder.DropColumn(
                name: "QrCodeImageContentType",
                table: "TrainBillDetails");

            migrationBuilder.DropColumn(
                name: "QrCodeImageData",
                table: "TrainBillDetails");

            migrationBuilder.DropColumn(
                name: "QrCodeImageUrl",
                table: "TrainBillDetails");
        }
    }
}
