# Propuesta de frontend (Blazor) alineada al examen técnico

## Objetivo

Definir una propuesta de pantallas frontend simple e intuitiva para cumplir los requerimientos del examen: carga de archivos CSV con bitácora, visualización de países, visualización de población por país, consultas relevantes y función de agregación.

## 1) Enfoque general de UX

1. Interfaz limpia con navegación lateral o superior de 4 a 5 opciones.
2. Flujo principal guiado: **Importar datos → Consultar países → Consultar población → Analizar**.
3. Formularios cortos, validaciones visibles y mensajes claros de éxito/error.
4. Tablas con paginación, búsqueda y ordenación para facilitar lectura.
5. Diseño orientado a escritorio, manteniendo responsive básico.

## 2) Propuesta de páginas

## 2.1 Página: Inicio / Dashboard

Propósito: punto de entrada rápido al sistema.

Contenido sugerido:
- Tarjetas de resumen:
  - total de entidades geográficas,
  - total de observaciones cargadas,
  - última carga ejecutada y su estado.
- Acciones rápidas:
  - "Cargar países",
  - "Cargar población",
  - "Ir a analítica".

Valor: permite saber estado del sistema en segundos.

## 2.2 Página: Carga de datos (CSV)

Propósito: ejecutar y monitorear la carga de archivos solicitada en el PDF.

Secciones sugeridas:
1. **Carga de países**
   - selector de archivo `.csv`,
   - botón "Procesar carga",
   - resultado (`ImportResult`: procesados, insertados, actualizados, omitidos, errores).
2. **Carga de población**
   - selector de archivo `.csv`,
   - botón "Procesar carga",
   - mismo resumen de resultado.
3. **Bitácora reciente de cargas**
   - listado con fecha, fuente, estado, detalle.

Reglas UX:
- Mostrar validaciones inmediatas (archivo vacío/no seleccionado).
- Permitir descargar o copiar detalle de errores si existen.
- Diferenciar visualmente éxito y fallo.

## 2.3 Página: Países

Propósito: visualizar datos de países.

Contenido sugerido:
- Tabla paginada con columnas:
  - `Alpha2Code`,
  - `Alpha3Code`,
  - `NameEnglish`,
  - `NameSpanish`,
  - `EntityType`,
  - `IsIsoCountry`.
- Filtros:
  - por `Alpha2Code` (opcional),
  - por `Alpha3Code` (principal),
  - por nombre.
- Acción de ver detalle de país.

Valor: cumple requerimiento de visualización de países y facilita validación de catálogo.

## 2.4 Página: Población por país

Propósito: visualizar datos de población por país en distintos años.

Contenido sugerido:
- Filtros:
  - país (`Alpha3Code`),
  - rango de años (desde/hasta),
  - paginación.
- Dos vistas en la misma página:
  1. **Tabla** de observaciones (año, valor, indicador).
  2. **Gráfico de línea simple** para tendencia histórica.

Reglas UX:
- Si no hay datos, mostrar estado vacío claro.
- Manejar nulos de población sin romper la vista.

## 2.5 Página: Analítica / Agregación

Propósito: cubrir la función requerida en el PDF.

Formulario de consulta:
- selección múltiple de países,
- selección de años (lista o rango),
- criterio de agregación (`sum`, `avg`, `max`, `min`),
- orden (`asc`, `desc`),
- botón "Consultar".

Resultado:
- tabla con país y valor agregado,
- opción de ordenar en UI,
- posibilidad de exportación simple (fase posterior).

Valor: satisface explícitamente el requerimiento funcional de agregación parametrizada.

## 3) Navegación sugerida

Menú principal:
1. Inicio
2. Carga de datos
3. Países
4. Población
5. Analítica

Secuencia recomendada de uso para evaluador técnico:
1) Cargar archivos, 2) revisar bitácora, 3) validar países, 4) validar población, 5) ejecutar agregación.

## 4) Componentes UI recomendados (MudBlazor)

1. `MudLayout`, `MudAppBar`, `MudDrawer` para estructura general.
2. `MudCard` para métricas y resúmenes.
3. `MudTable` o `MudDataGrid` para listados paginados/filtrables.
4. `MudTextField`, `MudSelect`, `MudDatePicker`/controles numéricos para filtros.
5. `MudAlert`, `MudSnackbar` para mensajes de estado.
6. `MudChart` (línea) para tendencia de población.

Nota: mantener estilo visual consistente y sin sobrecarga gráfica.

## 5) Estados y mensajes que debe contemplar el frontend

1. Cargando (spinners/botones deshabilitados durante peticiones).
2. Éxito de operación (mensaje resumido con conteos).
3. Error de validación (mensaje cercano al campo).
4. Error de proceso (mensaje general y detalle opcional).
5. Sin datos (estado vacío amigable con acción sugerida).

## 6) Criterios de aceptación de frontend (MVP)

1. El usuario puede cargar ambos CSV desde UI y ver resultado con detalle.
2. El usuario puede ver listado de países con filtros básicos.
3. El usuario puede consultar población por país y rango de años.
4. El usuario puede ejecutar agregación por países/años/criterio/orden y ver resultado.
5. La navegación es clara, simple e intuitiva.

## 7) Alcance MVP vs fase posterior

### MVP (obligatorio para examen)
- Páginas descritas arriba,
- filtros y paginación básicos,
- tabla y gráfico simple,
- mensajes claros de carga y errores.

### Posterior (opcional)
- exportación CSV/Excel de resultados,
- historial completo de bitácoras con filtros avanzados,
- internacionalización UI (ES/EN),
- mejoras de accesibilidad y temas.
