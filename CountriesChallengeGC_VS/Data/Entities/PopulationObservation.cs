namespace CountriesChallengeGC_VS.Data.Entities;

public class PopulationObservation
{
    public int EntityId { get; set; }
    public int IndicatorId { get; set; }
    public int? DataSourceId { get; set; }
    public short Year { get; set; }
    public long? PopulationValue { get; set; }

    public GeographicEntity GeographicEntity { get; set; } = default!;
    public Indicator Indicator { get; set; } = default!;
    public DataSource? DataSource { get; set; }
}
