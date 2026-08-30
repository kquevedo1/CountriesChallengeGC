namespace CountriesChallengeGC_VS.Services;

public record ImportResult(
    bool IsSuccess,
    string SourceName,
    int Processed,
    int Inserted,
    int Updated,
    int Skipped,
    int Errors,
    IReadOnlyList<string> Messages);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CountryFilter(
    string? Alpha2Code,
    string? Alpha3Code,
    string? Name,
    int PageNumber = 1,
    int PageSize = 25);

public record PopulationFilter(
    string? Alpha3Code,
    int? FromYear,
    int? ToYear,
    int PageNumber = 1,
    int PageSize = 25);

public record CountryDto(
    string Alpha2Code,
    string Alpha3Code,
    string NameEnglish,
    string? NameSpanish,
    string EntityType,
    bool IsIsoCountry);

public record CountryDetailDto(
    int EntityId,
    string Alpha2Code,
    string Alpha3Code,
    string NameEnglish,
    string? NameSpanish,
    string EntityType,
    bool IsIsoCountry);

public record PopulationDto(
    string Alpha3Code,
    string CountryName,
    string IndicatorCode,
    int Year,
    long? PopulationValue);

public record PopulationSeriesDto(
    string Alpha3Code,
    string CountryName,
    int Year,
    long? PopulationValue);

public record AggregationRequest(
    IReadOnlyList<string> Countries,
    IReadOnlyList<int> Years,
    AggregationCriteria Criteria,
    SortOrder Order);

public record CountryAggregationDto(
    string Alpha3Code,
    string CountryName,
    double? AggregatedValue);
