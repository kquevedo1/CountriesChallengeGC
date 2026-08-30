namespace CountriesChallengeGC_VS.Services;

public interface ICountryService
{
    Task<PagedResult<CountryDto>> GetCountriesAsync(CountryFilter filter, CancellationToken ct);
    Task<CountryDetailDto?> GetCountryByCodeAsync(string alpha3Code, CancellationToken ct);
}
