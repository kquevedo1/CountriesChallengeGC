namespace CountriesChallengeGC_VS.Data.Entities;

public class DataSource
{
    public int DataSourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Details { get; set; }

    public ICollection<PopulationObservation> PopulationObservations { get; set; } = new List<PopulationObservation>();
}
