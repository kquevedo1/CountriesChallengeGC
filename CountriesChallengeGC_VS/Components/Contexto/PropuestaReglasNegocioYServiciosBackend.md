# Propuesta de reglas de negocio y servicios de backend

## Objetivo

Definir reglas de negocio y servicios de backend para cumplir el examen técnico: carga de CSV (países y población), trazabilidad con logs, visualización de datos, consultas relevantes y función de agregación parametrizable.

## 1) Reglas de negocio

### 1.1 Reglas de catálogo geográfico (`GeographicEntity`)

1. `Alpha3Code` es obligatorio, único y será la clave natural de integración entre archivos.
2. `Alpha2Code` puede ser nulo para agregados/regiones.
3. `NameEnglish` es obligatorio.
4. `NameSpanish` es opcional (si no existe traducción, se mantiene nulo).
5. `EntityType` debe ser uno de: `Country`, `Territory`, `Aggregate`.
6. `IsIsoCountry`:
   - `1` cuando exista correspondencia ISO válida de país.
   - `0` para regiones/agregados y cualquier código no país.

### 1.2 Reglas de indicador (`Indicator`)

1. `IndicatorCode` es obligatorio y único.
2. `IndicatorName` es obligatorio.
3. El sistema debe aceptar múltiples indicadores, aunque inicialmente se cargue `SP.POP.TOTL`.

### 1.3 Reglas de observación poblacional (`PopulationObservation`)

1. Unicidad obligatoria por (`EntityId`, `IndicatorId`, `Year`).
2. `Year` válido entre 1900 y 2100.
3. `PopulationValue` debe ser nulo o mayor/igual a 0.
4. Si el valor en CSV está vacío, registrar observación con `PopulationValue = NULL`.
5. Si llega duplicado de clave compuesta:
   - opción recomendada: actualizar valor (estrategia *upsert*) y registrar evento en log.

### 1.4 Reglas de carga de archivos CSV

1. Toda carga debe generar un registro en `DataSource` con:
   - `SourceName` (nombre archivo y tipo de proceso),
   - `LoadedAt`,
   - `Status` (`Exitosa`/`Fallida`),
   - `Details` (resumen y errores).
2. Validaciones mínimas previas:
   - existencia de archivo,
   - estructura de encabezados,
   - codificación legible,
   - filas no vacías.
3. Errores por fila no detienen todo el proceso; se acumulan en bitácora y se continúa si es posible.
4. Si ocurre error crítico (archivo ilegible, encabezados inválidos), finalizar la carga como `Fallida`.
5. Los procesos deben ser idempotentes a nivel de negocio (re-ejecutables sin crear basura lógica).

### 1.5 Reglas de integración entre CSV

1. Cruce principal: `population.Country Code` con `CountryCodes_.Alpha-3 code`.
2. Códigos presentes en población y no presentes en catálogo ISO deben clasificarse como `Aggregate`.
3. Códigos ISO sin datos de población pueden existir sin error.
4. Las discrepancias de nombre entre fuentes no invalidan la carga si el código coincide.

### 1.6 Reglas de consulta/agregación

1. La función debe recibir:
   - lista de países/códigos,
   - lista o rango de años,
   - criterio (`sum`, `avg`, `max`, `min`),
   - orden (`asc`, `desc`).
2. Debe ignorar valores `NULL` en agregaciones (comportamiento SQL estándar).
3. Si no hay datos para un país en años solicitados, retornar resultado vacío o valor nulo según contrato.
4. La entrada debe validarse:
   - criterios fuera de catálogo => error de validación,
   - años fuera de rango => error de validación,
   - lista vacía => error de validación.

## 2) Servicios de backend propuestos (versión simplificada para cumplir el PDF)

Para mantener el alcance enfocado en el examen técnico, se proponen únicamente 4 servicios de aplicación.

### 2.1 `IImportService`

Responsabilidad: carga de ambos archivos CSV y registro de bitácora del proceso.

Operaciones sugeridas:
- `Task<ImportResult> ImportCountriesAsync(string filePath, CancellationToken ct)`
- `Task<ImportResult> ImportPopulationAsync(string filePath, CancellationToken ct)`

Incluye internamente:
- lectura CSV con delimitador coma,
- validación de estructura y reglas de negocio,
- alta/actualización de entidades (`GeographicEntity`, `Indicator`, `PopulationObservation`),
- registro de estado y detalle en `DataSource`.

### 2.2 `ICountryService`

Responsabilidad: visualización de datos de países.

Operaciones sugeridas:
- `Task<PagedResult<CountryDto>> GetCountriesAsync(CountryFilter filter, CancellationToken ct)`
- `Task<CountryDetailDto?> GetCountryByCodeAsync(string alpha3Code, CancellationToken ct)`

Notas:
- `Alpha3Code` como identificador canónico.
- Filtro opcional por `Alpha2Code` cuando aplique.

### 2.3 `IPopulationService`

Responsabilidad: visualización de población por país y serie histórica.

Operaciones sugeridas:
- `Task<PagedResult<PopulationDto>> GetPopulationByCountryAsync(PopulationFilter filter, CancellationToken ct)`
- `Task<IReadOnlyList<PopulationSeriesDto>> GetCountrySeriesAsync(string alpha3Code, int fromYear, int toYear, CancellationToken ct)`

### 2.4 `IAnalyticsService`

Responsabilidad: consultas de valor y función de agregación solicitada en el examen.

Operaciones sugeridas:
- `Task<IReadOnlyList<CountryAggregationDto>> GetAggregatedPopulationAsync(AggregationRequest request, CancellationToken ct)`

Contrato de entrada sugerido (`AggregationRequest`):
- `IReadOnlyList<string> Countries` (Alpha-3),
- `IReadOnlyList<int> Years` (o `FromYear/ToYear`),
- `AggregationCriteria Criteria` (`Sum`, `Avg`, `Max`, `Min`),
- `SortOrder Order` (`Asc`, `Desc`).

## 2.5 Simplificaciones explícitas del MVP

1. No se define `IDataSourceLogService` separado; la consulta de bitácoras puede quedar dentro de `IImportService` en esta fase.
2. No se exponen como contratos separados `ICsvReaderService`, `IImportValidationService` ni `IImportErrorCollector`; su lógica queda interna en `IImportService`.
3. No se fuerza patrón `IUnitOfWork` en interfaz independiente; puede usarse directamente la transaccionalidad de `DbContext`.
4. Los repositorios pueden ser mínimos o incluso implícitos vía `DbContext`, siempre manteniendo reglas de negocio y trazabilidad.

## 3) Consultas SQL relevantes (alineadas al examen)

1. Top N países por población en un año.
2. Evolución histórica de un país por rango de años.
3. Diferencia absoluta y porcentual entre dos años por país.
4. Ranking por crecimiento en período.
5. Población agregada por tipo de entidad (`Country`, `Territory`, `Aggregate`).

## 4) Recomendaciones de diseño para Blazor + .NET 8

1. Exponer estos servicios vía capa de aplicación (casos de uso) y consumirlos desde componentes Blazor.
2. Mantener DTOs de entrada/salida separados de entidades EF.
3. Aplicar paginación, orden y filtros en consultas de grillas.
4. Estandarizar respuesta de importación:
   - totales procesados,
   - insertados,
   - actualizados,
   - omitidos,
   - errores.
5. Registrar eventos técnicos con `ILogger` además de `DataSource` para trazabilidad funcional.

## 5) Criterios de aceptación funcionales

1. Carga de países y población ejecutable desde la aplicación y con bitácora persistida.
2. Visualización de países y población por país con filtros básicos.
3. Disponibilidad de consultas SQL de valor analítico.
4. Función de agregación operativa con parámetros de países, años, criterio y orden.
5. Validaciones y mensajes de error claros para datos inválidos.
