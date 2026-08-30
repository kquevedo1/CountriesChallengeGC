namespace CountriesChallengeGC_VS.Services;

public interface IAnalyticsService
{
    Task<IReadOnlyList<CountryAggregationDto>> GetAggregatedPopulationAsync(AggregationRequest request, CancellationToken ct);
}
