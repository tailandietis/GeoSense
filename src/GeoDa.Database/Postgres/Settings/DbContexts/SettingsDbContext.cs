using GeoDa.Application.Settings.Repository.Dto;
using Microsoft.EntityFrameworkCore;

namespace GeoDa.Database.Postgres.Settings.DbContexts;

public class SettingsDbContext : DbContext
{
    readonly string _connectionString = "";

    public SettingsDbContext()
    {
    }

    public SettingsDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserDto> Users { get; set; } = null!;

    public virtual DbSet<ParameterChangeDto> ParameterChanges { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_connectionString, npgsqlOptionsAction => npgsqlOptionsAction.CommandTimeout(10));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDto>(entity =>
        {
            entity.ToTable("users", "geoda");

            entity.HasKey(e => e.Name)
               .HasName("users_pkey");

            entity.Property(e => e.Name)
                .ValueGeneratedNever()
                .HasColumnName("name");

            entity.Property(e => e.Password).HasColumnName("password");

            entity.Property(e => e.Role).HasColumnName("role");
        });

        modelBuilder.Entity<ParameterChangeDto>(entity =>
        {
            entity.ToTable("change_log", "geoda");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("nextval('change_log_id_seq'::regclass)");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.UserName).HasColumnName("user_name");

            entity.Property(e => e.ParameterName).HasColumnName("setting_name");

            entity.Property(e => e.OldValue).HasColumnName("old_value");

            entity.Property(e => e.NewValue).HasColumnName("new_value");
        });
    }
}