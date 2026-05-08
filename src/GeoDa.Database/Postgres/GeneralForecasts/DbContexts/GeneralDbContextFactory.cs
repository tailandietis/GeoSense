namespace GeoDa.Database.Postgres.GeneralForecasts.DbContexts;

public static class GeneralDbContextFactory
{
    public static GeneralDbContext Create(string connectionString) =>
        new(connectionString);
}
