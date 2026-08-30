namespace CountriesChallengeGC_VS.Services;

public class PopulationService : IPopulationService
{
    public Task<PagedResult<PopulationDto>> GetPopulationByCountryAsync(PopulationFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var safeFilter = filter ?? new PopulationFilter(null, null, null);
        var result = new PagedResult<PopulationDto>(Array.Empty<PopulationDto>(), 0, safeFilter.PageNumber, safeFilter.PageSize);
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<PopulationSeriesDto>> GetCountrySeriesAsync(string alpha3Code, int fromYear, int toYear, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(alpha3Code) || fromYear > toYear)
        {
            return Task.FromResult<IReadOnlyList<PopulationSeriesDto>>(Array.Empty<PopulationSeriesDto>());
        }

        return Task.FromResult<IReadOnlyList<PopulationSeriesDto>>(Array.Empty<PopulationSeriesDto>());
    }
}
