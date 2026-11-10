using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leia.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class mig_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MenuItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MenuCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MenuCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuCategories");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_CategoryId",
                table: "MenuItems",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
