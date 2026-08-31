# Propuesta de planificación de la solución

## 1. Objetivo general

Construir una aplicación web en Blazor (.NET 8) para carga, trazabilidad, consulta y análisis de datos de países/población, integrada con SQL Server y alineada a los requerimientos del examen técnico.

## 2. Alcance implementado (MVP)

### Base de datos
- Modelo relacional en SQL Server:
  - `GeographicEntity`
  - `Indicator`
  - `DataSource`
  - `PopulationObservation`
- Restricciones, índices y llaves según DDL.

### Backend
- Configuración de conexión SQL Server con EF Core.
- `DbContext` y mapeo Fluent API del modelo.
- Servicios implementados:
  - `ImportService`: carga de CSV (países y población), upsert y log en `DataSource`.
  - `CountryService`: consulta de países con filtros/paginación.
  - `PopulationService`: consulta de población por país/rango de años.
  - `AnalyticsService`: agregaciones (`sum`, `avg`, `max`, `min`) y orden (`asc`, `desc`).

### Frontend
- Layout principal con navegación lateral y cabecera.
- Páginas implementadas:
  - Inicio (dashboard de resumen)
  - Carga de datos
  - Países
  - Población
  - Analítica
- Integración frontend-backend mediante inyección de servicios.
- Feedback de carga (estado en proceso, resultado y bitácora persistida).

## 3. Planificación propuesta por fases

## Fase 0: Descubrimiento y definición
- Analizar PDF, DDL y CSV.
- Definir reglas de negocio.
- Validar alcance MVP vs mejoras posteriores.

**Entregables**
- Documento de reglas y servicios backend.
- Documento de propuesta frontend.

## Fase 1: Fundaciones técnicas
- Configurar solución Blazor .NET 8.
- Configurar dependencias (MudBlazor y EF Core SQL Server).
- Configurar `appsettings` y DI.

**Entregables**
- Proyecto compilando con infraestructura base.

## Fase 2: Modelo de datos e integración BD
- Crear entidades y `DbContext`.
- Mapear constraints/índices/relaciones según DDL.
- Validar conectividad SQL Server.

**Entregables**
- Capa de datos operativa y estable.

## Fase 3: Backend funcional (casos de uso)
- Implementar importación de países.
- Implementar importación de población.
- Implementar consultas para páginas.
- Implementar analítica de agregación.

**Entregables**
- Servicios funcionales con persistencia real.

## Fase 4: Frontend funcional
- Implementar páginas y navegación.
- Conectar páginas a servicios reales.
- Ajustar UX: carga en progreso, alertas y estados vacíos.

**Entregables**
- Flujo end-to-end desde UI hasta BD.

## Fase 5: Validación y estabilización
- Pruebas funcionales manuales por pantalla.
- Ajuste de errores de circuito/traducción LINQ.
- Confirmar trazabilidad de carga en `DataSource`.

**Entregables**
- MVP estable y validado.

## 4. Flujo operativo recomendado del usuario final

1. Ir a **Carga de datos**.
2. Cargar CSV de países y luego CSV de población.
3. Revisar estado y detalle en historial de cargas.
4. Validar datos en **Países** y **Población**.
5. Ejecutar consultas en **Analítica** con países/años/criterio/orden.
6. Revisar resumen en **Inicio**.

## 5. Riesgos identificados y mitigación

1. **Errores de formato CSV**
   - Mitigación: validación de encabezados y manejo de errores por fila.

2. **Bloqueo de archivos temporales**
   - Mitigación: cierre explícito de streams y limpieza en `finally`.

3. **Errores de traducción LINQ (EF Core)**
   - Mitigación: simplificar consultas o materializar datos antes de agrupar.

4. **Pérdida de trazabilidad al navegar**
   - Mitigación: cargar historial desde `DataSource` (persistente).

## 6. Criterios de aceptación de la solución

1. Carga de ambos CSV desde interfaz y persistencia correcta.
2. Registro de cada carga en `DataSource` con estado y detalle.
3. Visualización de países y población con filtros.
4. Analítica operativa con `sum/avg/max/min` y `asc/desc`.
5. Dashboard de inicio mostrando resumen real.
6. Aplicación compilando y funcionando sin errores críticos de circuito.

## 7. Mejora continua (post-MVP)

- Exportación de resultados (CSV/Excel).
- Validaciones avanzadas de calidad de datos.
- Paginación y rendimiento optimizado para volúmenes grandes.
- Pruebas automáticas (unitarias/integración).
- Seguridad de credenciales (Secret Manager/variables de entorno).
