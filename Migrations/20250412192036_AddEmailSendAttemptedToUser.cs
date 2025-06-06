﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeafAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSendAttemptedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailSendAttempted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailSendAttempted",
                table: "AspNetUsers");
        }
    }
}
