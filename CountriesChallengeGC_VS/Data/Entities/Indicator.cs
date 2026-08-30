namespace CountriesChallengeGC_VS.Data.Entities;

public class Indicator
{
    public int IndicatorId { get; set; }
    public string IndicatorCode { get; set; } = string.Empty;
    public string IndicatorName { get; set; } = string.Empty;

    public ICollection<PopulationObservation> PopulationObservations { get; set; } = new List<PopulationObservation>();
}
