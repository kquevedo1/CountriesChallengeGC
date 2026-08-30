# Modelo Entidad-Relación (propuesto)

## Diagrama (Mermaid)

```mermaid
erDiagram
	GEOGRAPHIC_ENTITY ||--o{ POPULATION_OBSERVATION : "registra"
	INDICATOR ||--o{ POPULATION_OBSERVATION : "clasifica"
	DATA_SOURCE ||--o{ POPULATION_OBSERVATION : "origina"

	GEOGRAPHIC_ENTITY {
		int EntityId PK
		string Alpha3Code UK
		string Alpha2Code
		string NameEnglish
		string NameSpanish
		string EntityType
		bool IsIsoCountry
	}

	INDICATOR {
		int IndicatorId PK
		string IndicatorCode UK
		string IndicatorName
	}

	POPULATION_OBSERVATION {
		int EntityId FK
		int IndicatorId FK
		int DataSourceId FK
		smallint Year
		bigint PopulationValue
	}

	DATA_SOURCE {
		int DataSourceId PK
		string SourceName
		datetime LoadedAt
		string Status
		string Details
	}
```

## Notas

- `POPULATION_OBSERVATION` debe tener unicidad por (`EntityId`, `IndicatorId`, `Year`).
- `DATA_SOURCE` es opcional pero recomendable para trazabilidad de cargas.
