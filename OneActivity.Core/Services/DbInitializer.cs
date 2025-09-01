using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneActivity.Data;
using Microsoft.Data.Sqlite;
using System;

namespace OneActivity.Core.Services;

public class DbInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OneActivityDbContext>();

            _logger.LogInformation("Starting database initialization...");

            // Keep migrations types referenced so Android/AOT trimming does not remove them
            _ = typeof(OneActivity.Data.Migrations.InitialCreate);
            _ = typeof(OneActivity.Data.Migrations.ChangeTrainingEntryDateTimeToDateTimeOffset);
            _ = typeof(OneActivity.Data.Migrations.RenameTrainingToActivity);
            _ = typeof(OneActivity.Data.Migrations.AddGenderToUser);

            // Resilient baseline handling: if the app previously used EnsureCreated or old table names,
            // create the migrations history and mark earlier migrations as applied so we can migrate without data loss.
            try
            {
                using var conn = (SqliteConnection)dbContext.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                bool HasTable(string table)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$name";
                    cmd.Parameters.AddWithValue("$name", table);
                    var result = cmd.ExecuteScalar();
                    return result != null;
                }

                bool HasColumn(string table, string column)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"PRAGMA table_info({table});";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var name = reader.GetString(1);
                        if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return false;
                }

                bool migrationsHistoryExists = HasTable("__EFMigrationsHistory");
                bool hasTraining = HasTable("TrainingEntries");
                bool hasActivity = HasTable("ActivityEntries");
                bool hasUsers = HasTable("Users");
                bool hasGenderColumn = hasUsers && HasColumn("Users", "Gender");

                if (!migrationsHistoryExists && (hasTraining || hasActivity || hasUsers))
                {
                    _logger.LogInformation("Baselining migrations history for existing database schema (no data loss)…");

                    using var tx = conn.BeginTransaction();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
    MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
    ProductVersion TEXT NOT NULL
);";
                        cmd.ExecuteNonQuery();
                    }

                    void MarkApplied(string id, string productVersion)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = "INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ($id, $ver)";
                        cmd.Parameters.AddWithValue("$id", id);
                        cmd.Parameters.AddWithValue("$ver", productVersion);
                        cmd.ExecuteNonQuery();
                    }

                    // EF Core 8.0.19 per packages
                    const string ver = "8.0.19";

                    // If we have only the old schema (TrainingEntries, Users), mark initial + datetime change as applied
                    if (hasTraining && !hasActivity)
                    {
                        MarkApplied("20250824105501_InitialCreate", ver);
                        MarkApplied("20250901000000_ChangeTrainingEntryDateTimeToDateTimeOffset", ver);
                        _logger.LogInformation("Marked InitialCreate and ChangeTrainingEntryDateTimeToDateTimeOffset as applied.");
                    }

                    // If we already have ActivityEntries, mark known migrations as applied
                    if (hasActivity)
                    {
                        MarkApplied("20250824105501_InitialCreate", ver);
                        MarkApplied("20250901000000_ChangeTrainingEntryDateTimeToDateTimeOffset", ver);
                        MarkApplied("20250902000000_RenameTrainingToActivity", ver);
                        if (hasGenderColumn)
                        {
                            MarkApplied("20250903000000_AddGenderToUser", ver);
                        }
                        else
                        {
                            _logger.LogInformation("Users table lacks Gender column; AddGenderToUser migration will run.");
                        }
                        _logger.LogInformation("Marked known migrations as applied (ActivityEntries present).");
                    }

                    tx.Commit();
                }

                // If history exists but schema is still on old names, perform an in-place rename (idempotent) and mark migrations.
                if (migrationsHistoryExists && hasTraining && !hasActivity)
                {
                    _logger.LogInformation("Detected legacy schema with migrations history. Applying in-place rename to ActivityEntries…");
                    using var tx2 = conn.BeginTransaction();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx2;
                        cmd.CommandText = "ALTER TABLE TrainingEntries RENAME TO ActivityEntries;";
                        try { cmd.ExecuteNonQuery(); } catch { /* ignore if already renamed */ }
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx2;
                        cmd.CommandText = "ALTER TABLE ActivityEntries RENAME COLUMN NumberOfRepetitions TO Quantity;";
                        try { cmd.ExecuteNonQuery(); } catch { /* ignore if already renamed */ }
                    }

                    // Ensure history includes all known migrations so EF doesn't try to re-apply conflicting steps
                    void EnsureApplied(string id, string productVersion)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx2;
                        cmd.CommandText = "INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ($id, $ver)";
                        cmd.Parameters.AddWithValue("$id", id);
                        cmd.Parameters.AddWithValue("$ver", productVersion);
                        cmd.ExecuteNonQuery();
                    }
                    const string ver2 = "8.0.19";
                    EnsureApplied("20250824105501_InitialCreate", ver2);
                    EnsureApplied("20250901000000_ChangeTrainingEntryDateTimeToDateTimeOffset", ver2);
                    EnsureApplied("20250902000000_RenameTrainingToActivity", ver2);
                    tx2.Commit();
                    _logger.LogInformation("Legacy schema updated and migrations baselined.");
                }
            }
            catch (Exception exBaseline)
            {
                _logger.LogWarning(exBaseline, "Baseline check failed; proceeding with standard migrations.");
            }

            // Apply pending migrations
            dbContext.Database.Migrate();
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
        }
    }
}
