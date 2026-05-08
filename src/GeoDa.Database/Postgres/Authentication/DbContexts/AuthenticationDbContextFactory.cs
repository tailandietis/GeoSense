namespace GeoDa.Database.Postgres.Authentication.DbContexts;

public static class AuthenticationDbContextFactory
{
    public static AuthenticationDbContext Create(string connectionString) =>
       new(connectionString);
}
