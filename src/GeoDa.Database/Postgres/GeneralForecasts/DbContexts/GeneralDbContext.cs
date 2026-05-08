using GeoDa.Application.GeneralForecasts.Repository.Geometries.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Geophones.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.GsParams.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.ObjectInfos.Dtos;
using GeoDa.Application.GeneralForecasts.Repository.Qualsgrs.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GeoDa.Database.Postgres.GeneralForecasts.DbContexts;

public partial class GeneralDbContext : DbContext
{
    readonly string _connectionString = "";

    public GeneralDbContext()
    {
    }

    public GeneralDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public GeneralDbContext(DbContextOptions<GeneralDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GeometryDto> Geometrs { get; set; } = null!;
    public virtual DbSet<GeophoneDto> Geophons { get; set; } = null!;
    public virtual DbSet<GsParamDto> GsParams { get; set; } = null!;
    public virtual DbSet<ObjectInfoDto> Objects { get; set; } = null!;
    public virtual DbSet<QualsgrDto> Qualsgrs { get; set; } = null!;
    public virtual DbSet<MsgLogDto> MsgLogs { get; set; } = null!;
    public virtual DbSet<MsgCodeTextDto> MsgCodeTexts { get; set; } = null!;
    public virtual DbSet<ErrorCodeTextDto> ErrorCodeTexts { get; set; } = null!;

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

        modelBuilder.Entity<GeometryDto>(entity =>
        {
            entity.HasKey(e => e.Obj)
                .HasName("geometr_pkey");

            // entity.ToTable("geometr", "peleng");
            entity.ToTable("geometr");

            entity.Property(e => e.Obj)
                .ValueGeneratedNever()
                .HasColumnName("obj");

            entity.Property(e => e.NgpMax).HasColumnName("ngp_max");

            entity.Property(e => e.WShtrek).HasColumnName("w_shtrek");

            entity.Property(e => e.XMax).HasColumnName("x_max");

            entity.Property(e => e.XMin).HasColumnName("x_min");

            entity.Property(e => e.YMax).HasColumnName("y_max");

            entity.Property(e => e.YMin).HasColumnName("y_min");

            entity.Property(e => e.ZbDir).HasColumnName("zb_dir");

            entity.Property(e => e.ZMin).HasColumnName("z_min");

            entity.Property(e => e.ZMax).HasColumnName("z_max");
        });

        modelBuilder.Entity<GeophoneDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Num })
                .HasName("geophon_pkey");

            // entity.ToTable("geophon", "peleng");
            entity.ToTable("geophon");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Num).HasColumnName("num");

            entity.Property(e => e.X).HasColumnName("x");

            entity.Property(e => e.Y).HasColumnName("y");

            entity.Property(e => e.Z).HasColumnName("z");
        });

        modelBuilder.Entity<GsParamDto>(entity =>
        {
            // entity.ToTable("gs_params", "peleng");
            entity.ToTable("gs_params");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("nextval('gs_params_id_seq'::regclass)");

            entity.Property(e => e.Dt).HasColumnName("dt");

            entity.Property(e => e.ObjName).HasColumnName("obj_name");

            entity.Property(e => e.ParName).HasColumnName("par_name");

            entity.Property(e => e.ParVal).HasColumnName("par_val");
        });

        modelBuilder.Entity<ObjectInfoDto>(entity =>
        {
            entity.HasKey(e => e.Obj)
                .HasName("objects_pkey");

            // entity.ToTable("objects", "peleng");
            entity.ToTable("objects");

            entity.Property(e => e.Obj)
                .ValueGeneratedNever()
                .HasColumnName("obj");

            entity.Property(e => e.ObjName)
                .HasMaxLength(80)
                .HasColumnName("obj_name")
                .IsFixedLength(true);

            entity.Property(e => e.ObjTyp).HasColumnName("obj_typ");
        });

        modelBuilder.Entity<QualsgrDto>(entity =>
        {
            entity.HasKey(e => new { e.Obj, e.Idat, e.Itim })
                .HasName("qualsgr_pkey");

            //entity.ToTable("qualsgr", "xseis");
            entity.ToTable("qualsgr");

            entity.Property(e => e.Obj).HasColumnName("obj");

            entity.Property(e => e.Idat).HasColumnName("idat");

            entity.Property(e => e.Itim).HasColumnName("itim");

            entity.Property(e => e.Amax).HasColumnName("amax");

            entity.Property(e => e.Amid).HasColumnName("amid");

            entity.Property(e => e.Condit).HasColumnName("condit");

            entity.Property(e => e.Err).HasColumnName("err");

            entity.Property(e => e.Filt).HasColumnName("filt");

            entity.Property(e => e.Gain).HasColumnName("gain");

            entity.Property(e => e.InclUsed).HasColumnName("incl_used");

            entity.Property(e => e.Ngp).HasColumnName("ngp");

            entity.Property(e => e.Nsam).HasColumnName("nsam");

            entity.Property(e => e.Qc).HasColumnName("qc");

            entity.Property(e => e.Si).HasColumnName("si");
        });

        modelBuilder.Entity<MsgLogDto>(entity =>
        {
            entity.HasKey(e => new { e.ObjectId, e.Date, e.Time, e.N })
                .HasName("msglog_pkey");
                        
            entity.ToTable("msglog");

            entity.Property(e => e.ObjectId).HasColumnName("obj");

            entity.Property(e => e.Date).HasColumnName("idat");

            entity.Property(e => e.Time).HasColumnName("itim");

            entity.Property(e => e.N).HasColumnName("n");

            entity.Property(e => e.MsgCode).HasColumnName("msg_cod");

            entity.Property(e => e.ErrorCode).HasColumnName("err");

            entity.Property(e => e.Information).HasColumnName("inf");
        });

        modelBuilder.Entity<MsgCodeTextDto>(entity =>
        {
            entity.HasKey(e => new { e.Code })
                .HasName("msg_code_text_pkey");

            entity.ToTable("msg_code_text");

            entity.Property(e => e.Code).HasColumnName("code");

            entity.Property(e => e.Text).HasColumnName("text");
        });

        modelBuilder.Entity<ErrorCodeTextDto>(entity =>
        {
            entity.HasKey(e => new { e.Code })
                .HasName("error_code_text_pkey");

            entity.ToTable("error_code_text");

            entity.Property(e => e.Code).HasColumnName("code");

            entity.Property(e => e.Text).HasColumnName("text");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
