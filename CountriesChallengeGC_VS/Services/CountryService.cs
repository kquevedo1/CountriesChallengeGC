namespace CountriesChallengeGC_VS.Services;

public class CountryService : ICountryService
{
    public Task<PagedResult<CountryDto>> GetCountriesAsync(CountryFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var safeFilter = filter ?? new CountryFilter(null, null, null);
        var result = new PagedResult<CountryDto>(Array.Empty<CountryDto>(), 0, safeFilter.PageNumber, safeFilter.PageSize);
        return Task.FromResult(result);
    }

    public Task<CountryDetailDto?> GetCountryByCodeAsync(string alpha3Code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(alpha3Code))
        {
            return Task.FromResult<CountryDetailDto?>(null);
        }

        return Task.FromResult<CountryDetailDto?>(null);
    }
}
