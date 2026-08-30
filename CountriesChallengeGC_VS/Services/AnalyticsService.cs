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

        var yearFilter = years.Select(y => (short)y).ToArray();

        var countries = request.Countries
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        if (countries.Length == 0)
        {
            return Array.Empty<CountryAggregationDto>();
        }

        var baseData = await dbContext.PopulationObservations
            .AsNoTracking()
            .Where(x => countries.Contains(x.GeographicEntity.Alpha3Code)
                        && yearFilter.Contains(x.Year)
                        && x.PopulationValue.HasValue)
            .Select(x => new
            {
                x.GeographicEntity.Alpha3Code,
                x.GeographicEntity.NameEnglish,
                Value = x.PopulationValue!.Value
            })
            .ToListAsync(ct);

        var grouped = baseData.GroupBy(x => new { x.Alpha3Code, x.NameEnglish });

        var result = request.Criteria switch
        {
            AggregationCriteria.Sum => grouped.Select(g => new CountryAggregationDto(g.Key.Alpha3Code, g.Key.NameEnglish, g.Sum(x => (double)x.Value))).ToList(),
            AggregationCriteria.Avg => grouped.Select(g => new CountryAggregationDto(g.Key.Alpha3Code, g.Key.NameEnglish, g.Average(x => (double)x.Value))).ToList(),
            AggregationCriteria.Max => grouped.Select(g => new CountryAggregationDto(g.Key.Alpha3Code, g.Key.NameEnglish, g.Max(x => (double)x.Value))).ToList(),
            AggregationCriteria.Min => grouped.Select(g => new CountryAggregationDto(g.Key.Alpha3Code, g.Key.NameEnglish, g.Min(x => (double)x.Value))).ToList(),
            _ => grouped.Select(g => new CountryAggregationDto(g.Key.Alpha3Code, g.Key.NameEnglish, g.Sum(x => (double)x.Value))).ToList()
        };

        return request.Order == SortOrder.Asc
            ? result.OrderBy(x => x.AggregatedValue).ToList()
            : result.OrderByDescending(x => x.AggregatedValue).ToList();
    }
}
