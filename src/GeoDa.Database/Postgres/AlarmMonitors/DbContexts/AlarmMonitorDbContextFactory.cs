namespace GeoDa.Database.Postgres.AlarmMonitors.DbContexts;

public static class AlarmMonitorDbContextFactory
{
    public static AlarmMonitorDbContext Create(string connectionString) =>
        new(connectionString);
}
