namespace CountriesChallengeGC_VS.Services;

public interface IPopulationService
{
    Task<PagedResult<PopulationDto>> GetPopulationByCountryAsync(PopulationFilter filter, CancellationToken ct);
    Task<IReadOnlyList<PopulationSeriesDto>> GetCountrySeriesAsync(string alpha3Code, int fromYear, int toYear, CancellationToken ct);
}
