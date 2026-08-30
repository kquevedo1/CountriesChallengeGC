namespace CountriesChallengeGC_VS.Services;

public class ImportService : IImportService
{
    public Task<ImportResult> ImportCountriesAsync(string filePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var messages = new List<string>();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            messages.Add("El path del archivo es obligatorio.");
            return Task.FromResult(new ImportResult(false, "CountryCodes", 0, 0, 0, 0, 1, messages));
        }

        if (!File.Exists(filePath))
        {
            messages.Add("El archivo de países no existe.");
            return Task.FromResult(new ImportResult(false, "CountryCodes", 0, 0, 0, 0, 1, messages));
        }

        messages.Add("Servicio base de importación creado. Pendiente de implementación de persistencia.");
        return Task.FromResult(new ImportResult(true, "CountryCodes", 0, 0, 0, 0, 0, messages));
    }

    public Task<ImportResult> ImportPopulationAsync(string filePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var messages = new List<string>();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            messages.Add("El path del archivo es obligatorio.");
            return Task.FromResult(new ImportResult(false, "Population", 0, 0, 0, 0, 1, messages));
        }

        if (!File.Exists(filePath))
        {
            messages.Add("El archivo de población no existe.");
            return Task.FromResult(new ImportResult(false, "Population", 0, 0, 0, 0, 1, messages));
        }

        messages.Add("Servicio base de importación creado. Pendiente de implementación de persistencia.");
        return Task.FromResult(new ImportResult(true, "Population", 0, 0, 0, 0, 0, messages));
    }
}
