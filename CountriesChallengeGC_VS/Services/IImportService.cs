namespace CountriesChallengeGC_VS.Services;

public interface IImportService
{
    Task<ImportResult> ImportCountriesAsync(string filePath, CancellationToken ct);
    Task<ImportResult> ImportPopulationAsync(string filePath, CancellationToken ct);
    Task<IReadOnlyList<ImportLogDto>> GetImportLogsAsync(int take, CancellationToken ct);
}
