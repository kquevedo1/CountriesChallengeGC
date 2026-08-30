using CountriesChallengeGC_VS.Data;
using Microsoft.EntityFrameworkCore;

namespace CountriesChallengeGC_VS.Services;

public class CountryService(CountriesDbContext dbContext) : ICountryService
{
    public async Task<PagedResult<CountryDto>> GetCountriesAsync(CountryFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var safeFilter = filter ?? new CountryFilter(null, null, null);
        var pageNumber = safeFilter.PageNumber <= 0 ? 1 : safeFilter.PageNumber;
        var pageSize = safeFilter.PageSize <= 0 ? 25 : safeFilter.PageSize;

        var query = dbContext.GeographicEntities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(safeFilter.Alpha2Code))
        {
            var alpha2 = safeFilter.Alpha2Code.Trim().ToUpperInvariant();
            query = query.Where(x => x.Alpha2Code != null && x.Alpha2Code.ToUpper() == alpha2);
        }

        if (!string.IsNullOrWhiteSpace(safeFilter.Alpha3Code))
        {
            var alpha3 = safeFilter.Alpha3Code.Trim().ToUpperInvariant();
            query = query.Where(x => x.Alpha3Code.ToUpper() == alpha3);
        }

        if (!string.IsNullOrWhiteSpace(safeFilter.Name))
        {
            var name = safeFilter.Name.Trim();
            query = query.Where(x => x.NameEnglish.Contains(name) || (x.NameSpanish != null && x.NameSpanish.Contains(name)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.NameEnglish)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CountryDto(
                x.Alpha2Code ?? string.Empty,
                x.Alpha3Code,
                x.NameEnglish,
                x.NameSpanish,
                x.EntityType,
                x.IsIsoCountry))
            .ToListAsync(ct);

        return new PagedResult<CountryDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<CountryDetailDto?> GetCountryByCodeAsync(string alpha3Code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(alpha3Code))
        {
            return null;
        }

        var normalized = alpha3Code.Trim().ToUpperInvariant();

        return await dbContext.GeographicEntities
            .AsNoTracking()
            .Where(x => x.Alpha3Code.ToUpper() == normalized)
            .Select(x => new CountryDetailDto(
                x.EntityId,
                x.Alpha2Code ?? string.Empty,
                x.Alpha3Code,
                x.NameEnglish,
                x.NameSpanish,
                x.EntityType,
                x.IsIsoCountry))
            .FirstOrDefaultAsync(ct);
    }
}
