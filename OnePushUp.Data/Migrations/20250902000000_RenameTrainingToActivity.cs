using Microsoft.EntityFrameworkCore.Migrations;

namespace OnePushUp.Data.Migrations
{
    public partial class RenameTrainingToActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE TrainingEntries RENAME TO ActivityEntries;");
            migrationBuilder.Sql("ALTER TABLE ActivityEntries RENAME COLUMN NumberOfRepetitions TO Quantity;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE ActivityEntries RENAME COLUMN Quantity TO NumberOfRepetitions;");
            migrationBuilder.Sql("ALTER TABLE ActivityEntries RENAME TO TrainingEntries;");
        }
    }
}

