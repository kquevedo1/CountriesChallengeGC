namespace CountriesChallengeGC_VS.Data.Entities;

public class GeographicEntity
{
    public int EntityId { get; set; }
    public string Alpha3Code { get; set; } = string.Empty;
    public string? Alpha2Code { get; set; }
    public string NameEnglish { get; set; } = string.Empty;
    public string? NameSpanish { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public bool IsIsoCountry { get; set; }

    public ICollection<PopulationObservation> PopulationObservations { get; set; } = new List<PopulationObservation>();
}
