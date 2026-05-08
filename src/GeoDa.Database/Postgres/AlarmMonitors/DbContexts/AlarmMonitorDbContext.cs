using GeoDa.Application.AlarmMonitors.Repository.AlarmCodeDescriptions.Dtos;
using GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GeoDa.Database.Postgres.AlarmMonitors.DbContexts;

public partial class AlarmMonitorDbContext : DbContext
{
    readonly string _connectionString = "";

    public AlarmMonitorDbContext()
    {
    }

    public AlarmMonitorDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public AlarmMonitorDbContext(DbContextOptions<AlarmMonitorDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlarmItemDto> Alarms { get; set; } = null!;
    public virtual DbSet<AlarmCodeDescriptionDto> AlarmsCodes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_connectionString, npgsqlOptionsAction => npgsqlOptionsAction.CommandTimeout(10));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "Russian_Russia.1251");

        modelBuilder.Entity<AlarmItemDto>(entity =>
        {
            entity.ToTable("alarms", "geoda");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("nextval('alarms_id_seq'::regclass)");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.ErrorCode).HasColumnName("error_code");

            entity.Property(e => e.ModuleFamilyCode).HasColumnName("module_id");

            entity.Property(e => e.ObjectId).HasColumnName("obj_id");

            entity.Property(e => e.ServiceFamilyCode).HasColumnName("service_id");

            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<AlarmCodeDescriptionDto>(entity =>
        {
            entity.HasKey(e => new { e.ModuleFamilyCode, e.ServiceFamilyCode, e.ErrorCode })
                .HasName("alarm_codes_pkey");

            entity.ToTable("alarm_codes", "geoda");

            entity.Property(e => e.ModuleFamilyCode).HasColumnName("module_id");

            entity.Property(e => e.ServiceFamilyCode).HasColumnName("service_id");

            entity.Property(e => e.ErrorCode).HasColumnName("error_code");

            entity.Property(e => e.Msg)
                .IsRequired()
                .HasColumnName("msg");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
