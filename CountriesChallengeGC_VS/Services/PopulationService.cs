using CountriesChallengeGC_VS.Data;
using Microsoft.EntityFrameworkCore;

namespace CountriesChallengeGC_VS.Services;

public class PopulationService(CountriesDbContext dbContext) : IPopulationService
{
    public async Task<PagedResult<PopulationDto>> GetPopulationByCountryAsync(PopulationFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var safeFilter = filter ?? new PopulationFilter(null, null, null);
        var pageNumber = safeFilter.PageNumber <= 0 ? 1 : safeFilter.PageNumber;
        var pageSize = safeFilter.PageSize <= 0 ? 25 : safeFilter.PageSize;

        var query = dbContext.PopulationObservations
            .AsNoTracking()
            .Include(x => x.GeographicEntity)
            .Include(x => x.Indicator)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(safeFilter.Alpha3Code))
        {
            var alpha3 = safeFilter.Alpha3Code.Trim().ToUpperInvariant();
            query = query.Where(x => x.GeographicEntity.Alpha3Code.ToUpper() == alpha3);
        }

        if (safeFilter.FromYear.HasValue)
        {
            var fromYear = Math.Max(1900, safeFilter.FromYear.Value);
            query = query.Where(x => x.Year >= fromYear);
        }

        if (safeFilter.ToYear.HasValue)
        {
            var toYear = Math.Min(2100, safeFilter.ToYear.Value);
            query = query.Where(x => x.Year <= toYear);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.GeographicEntity.NameEnglish)
            .ThenBy(x => x.Year)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PopulationDto(
                x.GeographicEntity.Alpha3Code,
                x.GeographicEntity.NameEnglish,
                x.Indicator.IndicatorCode,
                x.Year,
                x.PopulationValue))
            .ToListAsync(ct);

        return new PagedResult<PopulationDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<PopulationSeriesDto>> GetCountrySeriesAsync(string alpha3Code, int fromYear, int toYear, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(alpha3Code) || fromYear > toYear)
        {
            return Array.Empty<PopulationSeriesDto>();
        }

        var normalizedCode = alpha3Code.Trim().ToUpperInvariant();
        var startYear = Math.Max(1900, fromYear);
        var endYear = Math.Min(2100, toYear);

        var series = await dbContext.PopulationObservations
            .AsNoTracking()
            .Include(x => x.GeographicEntity)
            .Where(x => x.GeographicEntity.Alpha3Code.ToUpper() == normalizedCode
                        && x.Year >= startYear
                        && x.Year <= endYear)
            .OrderBy(x => x.Year)
            .Select(x => new PopulationSeriesDto(
                x.GeographicEntity.Alpha3Code,
                x.GeographicEntity.NameEnglish,
                x.Year,
                x.PopulationValue))
            .ToListAsync(ct);

        return series;
    }
}
