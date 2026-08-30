namespace CountriesChallengeGC_VS.Services;

public class AnalyticsService : IAnalyticsService
{
    public Task<IReadOnlyList<CountryAggregationDto>> GetAggregatedPopulationAsync(AggregationRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (request.Countries.Count == 0 || request.Years.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<CountryAggregationDto>>(Array.Empty<CountryAggregationDto>());
        }

        if (request.Years.Any(y => y is < 1900 or > 2100))
        {
            return Task.FromResult<IReadOnlyList<CountryAggregationDto>>(Array.Empty<CountryAggregationDto>());
        }

        return Task.FromResult<IReadOnlyList<CountryAggregationDto>>(Array.Empty<CountryAggregationDto>());
    }
}
