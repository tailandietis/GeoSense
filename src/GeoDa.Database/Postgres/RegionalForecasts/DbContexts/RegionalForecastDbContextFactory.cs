namespace GeoDa.Database.Postgres.RegionalForecasts.DbContexts;

public static class RegionalForecastDbContextFactory
{
    public static RegionalForecastDbContext Create(string connectionString) =>
        new(connectionString);
}
