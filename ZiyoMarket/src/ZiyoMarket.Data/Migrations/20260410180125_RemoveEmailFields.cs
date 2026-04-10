using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiyoMarket.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductLikes");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsEmailSent",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DeliveryPartners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupportMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductLikes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailSent",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DeliveryPartners",
                type: "text",
                nullable: true);
        }
    }
}
