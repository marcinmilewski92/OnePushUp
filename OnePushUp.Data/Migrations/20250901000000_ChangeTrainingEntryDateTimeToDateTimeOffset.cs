using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePushUp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTrainingEntryDateTimeToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a temporary column to hold the converted values
            migrationBuilder.AddColumn<string>(
                name: "DateTime_Temp",
                table: "TrainingEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Copy data from DateTime to DateTime_Temp, preserving the value
            migrationBuilder.Sql(@"
                UPDATE TrainingEntries 
                SET DateTime_Temp = DateTime
            ");

            // Drop the original DateTime column
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "TrainingEntries");

            // Add the new DateTimeOffset column
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateTime",
                table: "TrainingEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero));

            // Copy data from the temp column to the new DateTimeOffset column
            migrationBuilder.Sql(@"
                UPDATE TrainingEntries 
                SET DateTime = DateTime_Temp
            ");

            // Drop the temporary column
            migrationBuilder.DropColumn(
                name: "DateTime_Temp",
                table: "TrainingEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Create a temporary column to hold the converted values
            migrationBuilder.AddColumn<string>(
                name: "DateTime_Temp",
                table: "TrainingEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Copy data from DateTimeOffset to DateTime_Temp
            migrationBuilder.Sql(@"
                UPDATE TrainingEntries 
                SET DateTime_Temp = DateTime
            ");

            // Drop the DateTimeOffset column
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "TrainingEntries");

            // Add the DateTime column back
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "TrainingEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

            // Copy data from the temp column to the DateTime column
            migrationBuilder.Sql(@"
                UPDATE TrainingEntries 
                SET DateTime = DateTime_Temp
            ");

            // Drop the temporary column
            migrationBuilder.DropColumn(
                name: "DateTime_Temp",
                table: "TrainingEntries");
        }
    }
}
