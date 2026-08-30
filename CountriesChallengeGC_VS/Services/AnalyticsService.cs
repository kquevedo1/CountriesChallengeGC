using CountriesChallengeGC_VS.Data;
using Microsoft.EntityFrameworkCore;

namespace CountriesChallengeGC_VS.Services;

public class AnalyticsService(CountriesDbContext dbContext) : IAnalyticsService
{
    public async Task<IReadOnlyList<CountryAggregationDto>> GetAggregatedPopulationAsync(AggregationRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (request.Countries.Count == 0 || request.Years.Count == 0)
        {
            return Array.Empty<CountryAggregationDto>();
        }

        var years = request.Years.Distinct().ToArray();
        if (years.Any(y => y is < 1900 or > 2100))
        {
            return Array.Empty<CountryAggregationDto>();
        }

        var countries = request.Countries
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        if (countries.Length == 0)
        {
            return Array.Empty<CountryAggregationDto>();
        }

        var baseQuery = dbContext.PopulationObservations
            .AsNoTracking()
            .Include(x => x.GeographicEntity)
            .Where(x => countries.Contains(x.GeographicEntity.Alpha3Code.ToUpper())
                        && years.Contains(x.Year)
                        && x.PopulationValue.HasValue)
            .GroupBy(x => new { x.GeographicEntity.Alpha3Code, x.GeographicEntity.NameEnglish });

        IQueryable<CountryAggregationDto> aggregationQuery = request.Criteria switch
        {
            AggregationCriteria.Sum => baseQuery.Select(g => new CountryAggregationDto(
                g.Key.Alpha3Code,
                g.Key.NameEnglish,
                g.Sum(x => (double?)x.PopulationValue))),

            AggregationCriteria.Avg => baseQuery.Select(g => new CountryAggregationDto(
                g.Key.Alpha3Code,
                g.Key.NameEnglish,
                g.Average(x => (double?)x.PopulationValue))),

            AggregationCriteria.Max => baseQuery.Select(g => new CountryAggregationDto(
                g.Key.Alpha3Code,
                g.Key.NameEnglish,
                g.Max(x => (double?)x.PopulationValue))),

            AggregationCriteria.Min => baseQuery.Select(g => new CountryAggregationDto(
                g.Key.Alpha3Code,
                g.Key.NameEnglish,
                g.Min(x => (double?)x.PopulationValue))),

            _ => baseQuery.Select(g => new CountryAggregationDto(
                g.Key.Alpha3Code,
                g.Key.NameEnglish,
                g.Sum(x => (double?)x.PopulationValue)))
        };

        aggregationQuery = request.Order == SortOrder.Asc
            ? aggregationQuery.OrderBy(x => x.AggregatedValue)
            : aggregationQuery.OrderByDescending(x => x.AggregatedValue);

        var result = await aggregationQuery.ToListAsync(ct);
        return result;
    }
}
