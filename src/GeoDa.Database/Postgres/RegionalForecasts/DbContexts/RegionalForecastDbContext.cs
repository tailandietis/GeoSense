using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsParamFEnergies.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GeoDa.Database.Postgres.RegionalForecasts.DbContexts;

public partial class RegionalForecastDbContext : DbContext
{
    readonly string _connectionString = "";

    public RegionalForecastDbContext()
    {
    }

    public RegionalForecastDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public RegionalForecastDbContext(DbContextOptions<RegionalForecastDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EventDto> Events { get; set; } = null!;
    public virtual DbSet<GsAlarmDto> GsAlarms { get; set; } = null!;
    public virtual DbSet<GsStatDto> GsStats { get; set; } = null!;
    public virtual DbSet<GsParamFEnergyDto> GsParamFEnergy { get; set; } = null!;

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

        modelBuilder.Entity<EventDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Idat, e.Itim, e.N })
                .HasName("events_pkey");

            entity.ToTable("events", "peleng");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Idat).HasColumnName("idat");

            entity.Property(e => e.Itim).HasColumnName("itim");

            entity.Property(e => e.N).HasColumnName("n");

            entity.Property(e => e.Ampl).HasColumnName("ampl");

            entity.Property(e => e.E).HasColumnName("e");

            entity.Property(e => e.Magn).HasColumnName("magn");

            entity.Property(e => e.Proc).HasColumnName("proc");

            entity.Property(e => e.X).HasColumnName("x");

            entity.Property(e => e.Y).HasColumnName("y");

            entity.Property(e => e.Z).HasColumnName("z");

            entity.Property(e => e.NpActual).HasColumnName("np_actual");
            entity.Property(e => e.RqMin).HasColumnName("rq_min");
            entity.Property(e => e.RqMax).HasColumnName("rq_max");
            entity.Property(e => e.GpActual).HasColumnName("gp_actual");
            entity.Property(e => e.AmplMax).HasColumnName("ampl_max");
            entity.Property(e => e.EMax).HasColumnName("e_max");
        });

        modelBuilder.Entity<GsAlarmDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Dt, e.Quality, e.AlarmStatus })
                .HasName("gs_alarms_pkey");

            entity.ToTable("gs_alarms", "peleng");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.Quality).HasColumnName("quality");

            entity.Property(e => e.AlarmStatus).HasColumnName("alarm_status");

            entity.Property(e => e.E).HasColumnName("e");

            entity.Property(e => e.ELim).HasColumnName("e_lim");

            entity.Property(e => e.CheckInterval).HasColumnName("check_interval");

            entity.Property(e => e.ItemDt).HasColumnName("item_dt");
        });

        modelBuilder.Entity<GsStatDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Dt, e.Quality })
                .HasName("rf_stat_pkey");

            entity.ToTable("gs_stat", "peleng");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.Quality).HasColumnName("quality");

            entity.Property(e => e.DtNewest).HasColumnName("dt_newest");

            entity.Property(e => e.DtOldest).HasColumnName("dt_oldest");

            entity.Property(e => e.MaxVal).HasColumnName("max_val");

            entity.Property(e => e.MinVal).HasColumnName("min_val");

            entity.Property(e => e.NRows).HasColumnName("n_rows");

            entity.Property(e => e.Q70).HasColumnName("q70");

            entity.Property(e => e.Q80).HasColumnName("q80");

            entity.Property(e => e.Q90).HasColumnName("q90");

            entity.Property(e => e.Q95).HasColumnName("q95");

            entity.Property(e => e.Q99).HasColumnName("q99");

            entity.Property(e => e.StatCalcInterval).HasColumnName("stat_calc_interval");
        });

        modelBuilder.Entity<GsParamFEnergyDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Dt })
                .HasName("gs_param_f_energy_pkey");

            entity.ToTable("gs_param_f_energy", "peleng");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.ParamFStatus).HasColumnName("f_status");

            entity.Property(e => e.EnergyStatus).HasColumnName("e_status");            
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
