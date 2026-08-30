using CountriesChallengeGC_VS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CountriesChallengeGC_VS.Data;

public class CountriesDbContext(DbContextOptions<CountriesDbContext> options) : DbContext(options)
{
    public DbSet<GeographicEntity> GeographicEntities => Set<GeographicEntity>();
    public DbSet<Indicator> Indicators => Set<Indicator>();
    public DbSet<DataSource> DataSources => Set<DataSource>();
    public DbSet<PopulationObservation> PopulationObservations => Set<PopulationObservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeographicEntity>(entity =>
        {
            entity.ToTable("GeographicEntity", "dbo");
            entity.HasKey(x => x.EntityId).HasName("PK_GeographicEntity");

            entity.Property(x => x.EntityId).ValueGeneratedOnAdd();
            entity.Property(x => x.Alpha3Code).HasColumnType("char(3)").IsRequired();
            entity.Property(x => x.Alpha2Code).HasColumnType("char(2)");
            entity.Property(x => x.NameEnglish).HasMaxLength(150).IsRequired();
            entity.Property(x => x.NameSpanish).HasMaxLength(150);
            entity.Property(x => x.EntityType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.IsIsoCountry).IsRequired();

            entity.HasIndex(x => x.Alpha3Code).IsUnique().HasDatabaseName("UQ_GeographicEntity_Alpha3Code");
            entity.ToTable(t => t.HasCheckConstraint("CK_GeographicEntity_EntityType", "EntityType IN (N'Country', N'Territory', N'Aggregate')"));
        });

        modelBuilder.Entity<Indicator>(entity =>
        {
            entity.ToTable("Indicator", "dbo");
            entity.HasKey(x => x.IndicatorId).HasName("PK_Indicator");

            entity.Property(x => x.IndicatorId).ValueGeneratedOnAdd();
            entity.Property(x => x.IndicatorCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.IndicatorName).HasMaxLength(200).IsRequired();

            entity.HasIndex(x => x.IndicatorCode).IsUnique().HasDatabaseName("UQ_Indicator_IndicatorCode");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.ToTable("DataSource", "dbo");
            entity.HasKey(x => x.DataSourceId).HasName("PK_DataSource");

            entity.Property(x => x.DataSourceId).ValueGeneratedOnAdd();
            entity.Property(x => x.SourceName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LoadedAt).HasColumnType("datetime2(0)").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(500);

            entity.ToTable(t => t.HasCheckConstraint("CK_DataSource_Status", "Status IN (N'Exitosa', N'Fallida')"));
        });

        modelBuilder.Entity<PopulationObservation>(entity =>
        {
            entity.ToTable("PopulationObservation", "dbo");
            entity.HasKey(x => new { x.EntityId, x.IndicatorId, x.Year }).HasName("PK_PopulationObservation");

            entity.Property(x => x.Year).HasColumnName("Year").IsRequired();
            entity.Property(x => x.PopulationValue).HasColumnType("bigint");

            entity.HasOne(x => x.GeographicEntity)
                .WithMany(x => x.PopulationObservations)
                .HasForeignKey(x => x.EntityId)
                .HasConstraintName("FK_PopulationObservation_GeographicEntity");

            entity.HasOne(x => x.Indicator)
                .WithMany(x => x.PopulationObservations)
                .HasForeignKey(x => x.IndicatorId)
                .HasConstraintName("FK_PopulationObservation_Indicator");

            entity.HasOne(x => x.DataSource)
                .WithMany(x => x.PopulationObservations)
                .HasForeignKey(x => x.DataSourceId)
                .HasConstraintName("FK_PopulationObservation_DataSource");

            entity.HasIndex(x => x.Year).HasDatabaseName("IX_PopulationObservation_Year");
            entity.HasIndex(x => new { x.IndicatorId, x.Year }).HasDatabaseName("IX_PopulationObservation_Indicator_Year");

            entity.ToTable(t => t.HasCheckConstraint("CK_PopulationObservation_Year", "[Year] BETWEEN 1900 AND 2100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PopulationObservation_Value", "PopulationValue IS NULL OR PopulationValue >= 0"));
        });
    }
}
