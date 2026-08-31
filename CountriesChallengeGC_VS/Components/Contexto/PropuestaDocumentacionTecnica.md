# Propuesta de Documentación Técnica del Software

## 1. Objetivo del documento técnico

Definir una documentación técnica clara, mantenible y útil para desarrolladores, QA y operación, que describa arquitectura, configuración, componentes, flujos, reglas de negocio, despliegue y troubleshooting de la solución.

## 2. Alcance de la documentación

La documentación debe cubrir:

1. Arquitectura general de la solución.
2. Diseño de base de datos.
3. Backend (servicios, contratos, lógica principal).
4. Frontend (páginas, componentes y flujo de usuario).
5. Configuración y ejecución local.
6. Pruebas funcionales mínimas.
7. Operación, monitoreo básico y resolución de errores comunes.

## 3. Estructura recomendada de la documentación

## 3.1 Resumen ejecutivo técnico

- Propósito de la aplicación.
- Tecnologías principales:
  - Blazor (.NET 8)
  - C#
  - SQL Server
  - MudBlazor
  - Entity Framework Core
- Alcance MVP implementado y alcance post-MVP.

## 3.2 Arquitectura de solución

- Diagrama de alto nivel:
  - UI (Blazor)
  - Capa de servicios
  - Capa de datos (EF Core + DbContext)
  - SQL Server
- Patrón usado en servicios (`IService` + `Service`).
- Flujo de dependencia (DI en `Program.cs`).

## 3.3 Modelo de datos

- Referencia a DDL oficial (`DDL_ModeloEntidadRelacion.sql`).
- Diccionario de tablas y columnas:
  - `GeographicEntity`
  - `Indicator`
  - `PopulationObservation`
  - `DataSource`
- Reglas de integridad (PK, FK, constraints, índices).
- Decisiones de diseño de datos (e.g., `Alpha3Code` como clave natural funcional).

## 3.4 Backend técnico

- Contratos de servicio:
  - `IImportService`
  - `ICountryService`
  - `IPopulationService`
  - `IAnalyticsService`
- Implementaciones concretas y responsabilidades.
- DTO/Records y Enums utilizados.
- Flujo detallado de importación:
  - validación archivo,
  - parseo CSV,
  - upsert,
  - logging en `DataSource`.
- Consideraciones de rendimiento y límites actuales.

## 3.5 Frontend técnico

- Estructura por páginas y carpetas.
- Layout principal y navegación.
- Pantallas implementadas:
  - Inicio
  - Carga de datos
  - Países
  - Población
  - Analítica
- Enlace de UI con backend (inyección de servicios).
- Estados de UX:
  - loading,
  - éxito,
  - error,
  - estado vacío.

## 3.6 Configuración y arranque

- Requisitos previos:
  - .NET SDK 8
  - SQL Server
- Configuración de `appsettings` y `ConnectionStrings`.
- Comandos o pasos para ejecutar localmente.
- Variables sensibles y recomendación de seguridad (no exponer credenciales en texto plano para ambientes reales).

## 3.7 Pruebas funcionales sugeridas

- Caso 1: carga de CSV de países.
- Caso 2: carga de CSV de población.
- Caso 3: validación de historial (`DataSource`).
- Caso 4: consulta de países con filtros y paginación.
- Caso 5: consulta de población con filtros y paginación.
- Caso 6: analítica (`sum/avg/max/min`, `asc/desc`).
- Resultado esperado por cada caso.

## 3.8 Manejo de errores y troubleshooting

- Errores típicos y solución:
  - archivo en uso durante carga,
  - fallas de conexión SQL,
  - errores de traducción LINQ/EF,
  - errores de JavaScript en MudBlazor por assets faltantes.
- Guía rápida para diagnóstico (logs app + salida depuración).

## 3.9 Decisiones técnicas y trade-offs

- Decisiones tomadas durante el desarrollo:
  - separación contratos/implementación,
  - persistencia de historial en BD,
  - cálculo analítico en memoria para evitar errores de traducción LINQ compleja.
- Riesgos conocidos y cómo mitigarlos.

## 3.10 Roadmap técnico posterior

- Seguridad y secretos de conexión.
- Pruebas unitarias e integración.
- Optimización de consultas para grandes volúmenes.
- Exportación de reportes.
- Mejora de observabilidad (telemetría).

## 4. Formato recomendado por archivo

Para mantener orden, se recomienda crear la documentación técnica en varios archivos dentro de `Contexto`:

1. `01_ResumenTecnico.md`
2. `02_Arquitectura.md`
3. `03_BaseDeDatos.md`
4. `04_Backend.md`
5. `05_Frontend.md`
6. `06_ConfiguracionYEjecucion.md`
7. `07_PruebasFuncionales.md`
8. `08_Troubleshooting.md`
9. `09_DecisionesTecnicas.md`
10. `10_Roadmap.md`

## 5. Criterios de calidad de la documentación

1. Debe estar alineada al código actual (no desactualizada).
2. Debe incluir ejemplos concretos de uso.
3. Debe permitir a un nuevo desarrollador levantar el proyecto sin asistencia adicional.
4. Debe explicar tanto el “qué” como el “por qué” de las decisiones clave.
5. Debe actualizarse junto con cambios funcionales relevantes.

## 6. Recomendación de mantenimiento

- Definir la documentación técnica como parte del flujo de entrega:
  - Todo cambio de backend/frontend/BD debe incluir actualización de documento asociado.
- Registrar en changelog técnico:
  - fecha,
  - cambio,
  - archivo impactado,
  - responsable.
