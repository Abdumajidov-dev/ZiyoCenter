using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiyoMarket.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashbackTransactions_Orders_UsedInOrderId",
                table: "CashbackTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CashbackTransactions_UsedInOrderId",
                table: "CashbackTransactions");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UsedInOrderId",
                table: "CashbackTransactions");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Orders",
                newName: "SellerNotes");

            migrationBuilder.RenameColumn(
                name: "CancellationReason",
                table: "Orders",
                newName: "PaymentReference");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "CashbackTransactions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CashbackTransactions",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "SupportMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "SupportMessages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "SupportMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "SupportChats",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosedAt",
                table: "SupportChats",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "SupportChats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "SupportChats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "SupportChats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resolution",
                table: "SupportChats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "SupportChats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "SupportChats",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderDate",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNotes",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaidAt",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "OrderDiscounts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AppliedBy",
                table: "OrderDiscounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UsedAt",
                table: "CashbackTransactions",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpiresAt",
                table: "CashbackTransactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "EarnedAt",
                table: "CashbackTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "CashbackTransactions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                table: "Admins",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "Resolution",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SupportChats");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "OrderDiscounts");

            migrationBuilder.DropColumn(
                name: "AppliedBy",
                table: "OrderDiscounts");

            migrationBuilder.DropColumn(
                name: "EarnedAt",
                table: "CashbackTransactions");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "CashbackTransactions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "SellerNotes",
                table: "Orders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "PaymentReference",
                table: "Orders",
                newName: "CancellationReason");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "CashbackTransactions",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CashbackTransactions",
                newName: "Notes");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "SupportChats",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClosedAt",
                table: "SupportChats",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UsedAt",
                table: "CashbackTransactions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "CashbackTransactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "UsedInOrderId",
                table: "CashbackTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashbackTransactions_UsedInOrderId",
                table: "CashbackTransactions",
                column: "UsedInOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashbackTransactions_Orders_UsedInOrderId",
                table: "CashbackTransactions",
                column: "UsedInOrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
