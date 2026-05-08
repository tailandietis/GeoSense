namespace GeoDa.Database.Postgres.Settings.DbContexts;

public static class SettingsDbContextFactory
{
    public static SettingsDbContext Create(string connectionString) =>
       new(connectionString);
}
