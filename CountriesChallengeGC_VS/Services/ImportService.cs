using System.Globalization;
using System.Text;
using CountriesChallengeGC_VS.Data;
using CountriesChallengeGC_VS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CountriesChallengeGC_VS.Services;

public class ImportService(CountriesDbContext dbContext) : IImportService
{
    private const int MaxDetailLength = 500;

    public async Task<IReadOnlyList<ImportLogDto>> GetImportLogsAsync(int take, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var safeTake = take <= 0 ? 50 : Math.Min(take, 200);

        return await dbContext.DataSources
            .AsNoTracking()
            .OrderByDescending(x => x.LoadedAt)
            .Take(safeTake)
            .Select(x => new ImportLogDto(
                x.LoadedAt,
                x.SourceName,
                x.Status,
                x.Details))
            .ToListAsync(ct);
    }

    public async Task<ImportResult> ImportCountriesAsync(string filePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var sourceName = "CountryCodes";
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new ImportResult(false, sourceName, 0, 0, 0, 0, 1, ["El path del archivo es obligatorio."]);
        }

        if (!File.Exists(filePath))
        {
            return new ImportResult(false, sourceName, 0, 0, 0, 0, 1, ["El archivo de países no existe."]);
        }

        var processed = 0;
        var inserted = 0;
        var updated = 0;
        var skipped = 0;
        var errors = 0;
        var messages = new List<string>();

        var dataSource = await CreateDataSourceAsync(sourceName, filePath, ct);

        try
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var headerLine = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                errors++;
                messages.Add("El archivo de países está vacío.");
                return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
            }

            var headers = ParseCsvLine(headerLine);
            var idxCountry = FindHeaderIndex(headers, "Country");
            var idxAlpha2 = FindHeaderIndex(headers, "Alpha-2 code");
            var idxAlpha3 = FindHeaderIndex(headers, "Alpha-3 code");

            if (idxCountry < 0 || idxAlpha2 < 0 || idxAlpha3 < 0)
            {
                errors++;
                messages.Add("Encabezados inválidos en archivo de países.");
                return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
            }

            var entitiesByAlpha3 = await dbContext.GeographicEntities
                .ToDictionaryAsync(x => x.Alpha3Code, StringComparer.OrdinalIgnoreCase, ct);

            while (!reader.EndOfStream)
            {
                ct.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line))
                {
                    skipped++;
                    continue;
                }

                var columns = ParseCsvLine(line);
                if (columns.Count <= Math.Max(idxCountry, Math.Max(idxAlpha2, idxAlpha3)))
                {
                    errors++;
                    AddError(messages, $"Fila inválida en países: '{line}'.");
                    continue;
                }

                var countryName = columns[idxCountry].Trim();
                var alpha2 = NormalizeNullable(columns[idxAlpha2]);
                var alpha3 = NormalizeNullable(columns[idxAlpha3])?.ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(alpha3) || alpha3.Length != 3)
                {
                    errors++;
                    AddError(messages, $"Alpha-3 inválido en fila de países: '{line}'.");
                    continue;
                }

                processed++;

                if (entitiesByAlpha3.TryGetValue(alpha3, out var existingEntity))
                {
                    var wasModified = false;
                    var normalizedName = string.IsNullOrWhiteSpace(countryName) ? alpha3 : countryName;

                    if (!string.Equals(existingEntity.Alpha2Code, alpha2, StringComparison.Ordinal))
                    {
                        existingEntity.Alpha2Code = alpha2;
                        wasModified = true;
                    }

                    if (!string.Equals(existingEntity.NameEnglish, normalizedName, StringComparison.Ordinal))
                    {
                        existingEntity.NameEnglish = normalizedName;
                        wasModified = true;
                    }

                    if (!string.Equals(existingEntity.EntityType, "Country", StringComparison.Ordinal))
                    {
                        existingEntity.EntityType = "Country";
                        wasModified = true;
                    }

                    if (!existingEntity.IsIsoCountry)
                    {
                        existingEntity.IsIsoCountry = true;
                        wasModified = true;
                    }

                    if (wasModified)
                    {
                        updated++;
                    }
                }
                else
                {
                    var newEntity = new GeographicEntity
                    {
                        Alpha2Code = alpha2,
                        Alpha3Code = alpha3,
                        NameEnglish = string.IsNullOrWhiteSpace(countryName) ? alpha3 : countryName,
                        NameSpanish = null,
                        EntityType = "Country",
                        IsIsoCountry = true
                    };

                    dbContext.GeographicEntities.Add(newEntity);
                    entitiesByAlpha3[alpha3] = newEntity;
                    inserted++;
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            errors++;
            AddError(messages, $"Error importando países: {ex.Message}");
        }

        return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
    }

    public async Task<ImportResult> ImportPopulationAsync(string filePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var sourceName = "Population";
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new ImportResult(false, sourceName, 0, 0, 0, 0, 1, ["El path del archivo es obligatorio."]);
        }

        if (!File.Exists(filePath))
        {
            return new ImportResult(false, sourceName, 0, 0, 0, 0, 1, ["El archivo de población no existe."]);
        }

        var processed = 0;
        var inserted = 0;
        var updated = 0;
        var skipped = 0;
        var errors = 0;
        var messages = new List<string>();

        var dataSource = await CreateDataSourceAsync(sourceName, filePath, ct);

        try
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var headerLine = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                errors++;
                messages.Add("El archivo de población está vacío.");
                return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
            }

            var headers = ParseCsvLine(headerLine);
            var idxCountryName = FindHeaderIndex(headers, "Country Name");
            var idxCountryCode = FindHeaderIndex(headers, "Country Code");
            var idxIndicatorName = FindHeaderIndex(headers, "Indicator Name");
            var idxIndicatorCode = FindHeaderIndex(headers, "Indicator Code");

            if (idxCountryName < 0 || idxCountryCode < 0 || idxIndicatorName < 0 || idxIndicatorCode < 0)
            {
                errors++;
                messages.Add("Encabezados inválidos en archivo de población.");
                return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
            }

            var yearColumns = headers
                .Select((h, i) => new { Header = h.Trim(), Index = i })
                .Where(x => short.TryParse(x.Header, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) && y is >= 1900 and <= 2100)
                .Select(x => new YearColumn(x.Index, short.Parse(x.Header, CultureInfo.InvariantCulture)))
                .ToArray();

            if (yearColumns.Length == 0)
            {
                errors++;
                messages.Add("No se encontraron columnas de año válidas en archivo de población.");
                return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
            }

            var entitiesByAlpha3 = await dbContext.GeographicEntities
                .ToDictionaryAsync(x => x.Alpha3Code, StringComparer.OrdinalIgnoreCase, ct);

            var indicatorsByCode = await dbContext.Indicators
                .ToDictionaryAsync(x => x.IndicatorCode, StringComparer.OrdinalIgnoreCase, ct);

            var observationKeys = await dbContext.PopulationObservations
                .ToDictionaryAsync(
                    x => ObservationKey.FromIds(x.EntityId, x.IndicatorId, x.Year),
                    x => x,
                    ct);

            while (!reader.EndOfStream)
            {
                ct.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line))
                {
                    skipped++;
                    continue;
                }

                var columns = ParseCsvLine(line);
                if (columns.Count <= Math.Max(idxIndicatorCode, idxCountryCode))
                {
                    errors++;
                    AddError(messages, $"Fila inválida en población: '{line}'.");
                    continue;
                }

                var countryName = NormalizeNullable(columns[idxCountryName]);
                var countryCode = NormalizeNullable(columns[idxCountryCode])?.ToUpperInvariant();
                var indicatorName = NormalizeNullable(columns[idxIndicatorName]);
                var indicatorCode = NormalizeNullable(columns[idxIndicatorCode]);

                if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 3)
                {
                    errors++;
                    AddError(messages, $"Country Code inválido en población: '{line}'.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(indicatorCode))
                {
                    errors++;
                    AddError(messages, $"Indicator Code inválido en población: '{line}'.");
                    continue;
                }

                if (!entitiesByAlpha3.TryGetValue(countryCode, out var entity))
                {
                    entity = new GeographicEntity
                    {
                        Alpha2Code = null,
                        Alpha3Code = countryCode,
                        NameEnglish = string.IsNullOrWhiteSpace(countryName) ? countryCode : countryName,
                        NameSpanish = null,
                        EntityType = "Aggregate",
                        IsIsoCountry = false
                    };

                    dbContext.GeographicEntities.Add(entity);
                    await dbContext.SaveChangesAsync(ct);
                    entitiesByAlpha3[countryCode] = entity;
                    inserted++;
                }

                if (!indicatorsByCode.TryGetValue(indicatorCode, out var indicator))
                {
                    indicator = new Indicator
                    {
                        IndicatorCode = indicatorCode,
                        IndicatorName = string.IsNullOrWhiteSpace(indicatorName) ? indicatorCode : indicatorName
                    };

                    dbContext.Indicators.Add(indicator);
                    await dbContext.SaveChangesAsync(ct);
                    indicatorsByCode[indicatorCode] = indicator;
                    inserted++;
                }

                foreach (var yearCol in yearColumns)
                {
                    if (yearCol.Index >= columns.Count)
                    {
                        continue;
                    }

                    processed++;
                    var valueText = NormalizeNullable(columns[yearCol.Index]);
                    long? populationValue = null;

                    if (!string.IsNullOrWhiteSpace(valueText))
                    {
                        if (!long.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
                        {
                            errors++;
                            AddError(messages, $"Valor de población inválido para {countryCode}-{yearCol.Year}: '{valueText}'.");
                            continue;
                        }

                        populationValue = parsedValue;
                    }

                    var key = ObservationKey.FromIds(entity.EntityId, indicator.IndicatorId, yearCol.Year);
                    if (observationKeys.TryGetValue(key, out var existingObservation))
                    {
                        var wasModified = false;
                        if (existingObservation.PopulationValue != populationValue)
                        {
                            existingObservation.PopulationValue = populationValue;
                            wasModified = true;
                        }

                        if (existingObservation.DataSourceId != dataSource.DataSourceId)
                        {
                            existingObservation.DataSourceId = dataSource.DataSourceId;
                            wasModified = true;
                        }

                        if (wasModified)
                        {
                            updated++;
                        }
                    }
                    else
                    {
                        var newObservation = new PopulationObservation
                        {
                            EntityId = entity.EntityId,
                            IndicatorId = indicator.IndicatorId,
                            DataSourceId = dataSource.DataSourceId,
                            Year = yearCol.Year,
                            PopulationValue = populationValue
                        };

                        dbContext.PopulationObservations.Add(newObservation);
                        observationKeys[key] = newObservation;
                        inserted++;
                    }
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            errors++;
            AddError(messages, $"Error importando población: {ex.Message}");
        }

        return await FinalizeImportAsync(dataSource, sourceName, processed, inserted, updated, skipped, errors, messages, ct);
    }

    private async Task<DataSource> CreateDataSourceAsync(string sourceName, string filePath, CancellationToken ct)
    {
        var dataSource = new DataSource
        {
            SourceName = $"{sourceName}:{Path.GetFileName(filePath)}",
            LoadedAt = DateTime.UtcNow,
            Status = "Fallida",
            Details = "Proceso iniciado"
        };

        dbContext.DataSources.Add(dataSource);
        await dbContext.SaveChangesAsync(ct);
        return dataSource;
    }

    private async Task<ImportResult> FinalizeImportAsync(
        DataSource dataSource,
        string sourceName,
        int processed,
        int inserted,
        int updated,
        int skipped,
        int errors,
        List<string> messages,
        CancellationToken ct)
    {
        var isSuccess = errors == 0;
        var status = isSuccess ? "Exitosa" : "Fallida";

        if (messages.Count == 0)
        {
            messages.Add(isSuccess
                ? "Importación completada correctamente."
                : "Importación finalizada con errores.");
        }

        dataSource.LoadedAt = DateTime.UtcNow;
        dataSource.Status = status;
        dataSource.Details = BuildDetails(processed, inserted, updated, skipped, errors, messages);

        await dbContext.SaveChangesAsync(ct);

        return new ImportResult(isSuccess, sourceName, processed, inserted, updated, skipped, errors, messages);
    }

    private static int FindHeaderIndex(IReadOnlyList<string> headers, string expected)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            if (string.Equals(headers[i].Trim(), expected, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString());
        return result;
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static void AddError(List<string> messages, string error)
    {
        if (messages.Count < 20)
        {
            messages.Add(error);
        }
    }

    private static string BuildDetails(int processed, int inserted, int updated, int skipped, int errors, List<string> messages)
    {
        var baseDetail = $"Procesados={processed}; Insertados={inserted}; Actualizados={updated}; Omitidos={skipped}; Errores={errors}";
        if (messages.Count == 0)
        {
            return TrimToMax(baseDetail);
        }

        var detail = $"{baseDetail}. Mensajes: {string.Join(" | ", messages)}";
        return TrimToMax(detail);
    }

    private static string TrimToMax(string value)
    {
        return value.Length <= MaxDetailLength
            ? value
            : value[..MaxDetailLength];
    }

    private readonly record struct YearColumn(int Index, short Year);

    private readonly record struct ObservationKey(int EntityId, int IndicatorId, short Year)
    {
        public static ObservationKey FromIds(int entityId, int indicatorId, short year)
            => new(entityId, indicatorId, year);
    }
}
