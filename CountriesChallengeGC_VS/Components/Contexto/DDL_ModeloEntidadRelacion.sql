SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'CountriesPopulationGuateCana') IS NULL
BEGIN
	CREATE DATABASE CountriesPopulationGuateCana;
END;
GO

USE CountriesPopulationGuateCana;
GO

CREATE TABLE dbo.GeographicEntity
(
	EntityId INT IDENTITY(1,1) NOT NULL,
	Alpha3Code CHAR(3) NOT NULL,
	Alpha2Code CHAR(2) NULL,
	NameEnglish NVARCHAR(150) NOT NULL,
	NameSpanish NVARCHAR(150) NULL,
	EntityType NVARCHAR(20) NOT NULL,
	IsIsoCountry BIT NOT NULL,
	CONSTRAINT PK_GeographicEntity PRIMARY KEY CLUSTERED (EntityId),
	CONSTRAINT UQ_GeographicEntity_Alpha3Code UNIQUE (Alpha3Code),
	CONSTRAINT CK_GeographicEntity_EntityType CHECK (EntityType IN (N'Country', N'Territory', N'Aggregate'))
);
GO

CREATE TABLE dbo.Indicator
(
	IndicatorId INT IDENTITY(1,1) NOT NULL,
	IndicatorCode NVARCHAR(30) NOT NULL,
	IndicatorName NVARCHAR(200) NOT NULL,
	CONSTRAINT PK_Indicator PRIMARY KEY CLUSTERED (IndicatorId),
	CONSTRAINT UQ_Indicator_IndicatorCode UNIQUE (IndicatorCode)
);
GO

CREATE TABLE dbo.DataSource
(
	DataSourceId INT IDENTITY(1,1) NOT NULL,
	SourceName NVARCHAR(200) NOT NULL,
	LoadedAt DATETIME2(0) NOT NULL,
	Status NVARCHAR(20) NOT NULL,
	Details NVARCHAR(500) NULL,
	CONSTRAINT PK_DataSource PRIMARY KEY CLUSTERED (DataSourceId),
	CONSTRAINT CK_DataSource_Status CHECK (Status IN (N'Exitosa', N'Fallida'))
);
GO

CREATE TABLE dbo.PopulationObservation
(
	EntityId INT NOT NULL,
	IndicatorId INT NOT NULL,
	DataSourceId INT NULL,
	[Year] SMALLINT NOT NULL,
	PopulationValue BIGINT NULL,
	CONSTRAINT PK_PopulationObservation PRIMARY KEY CLUSTERED (EntityId, IndicatorId, [Year]),
	CONSTRAINT FK_PopulationObservation_GeographicEntity FOREIGN KEY (EntityId)
		REFERENCES dbo.GeographicEntity (EntityId),
	CONSTRAINT FK_PopulationObservation_Indicator FOREIGN KEY (IndicatorId)
		REFERENCES dbo.Indicator (IndicatorId),
	CONSTRAINT FK_PopulationObservation_DataSource FOREIGN KEY (DataSourceId)
		REFERENCES dbo.DataSource (DataSourceId),
	CONSTRAINT CK_PopulationObservation_Year CHECK ([Year] BETWEEN 1900 AND 2100),
	CONSTRAINT CK_PopulationObservation_Value CHECK (PopulationValue IS NULL OR PopulationValue >= 0)
);
GO

CREATE INDEX IX_PopulationObservation_Year
	ON dbo.PopulationObservation ([Year]);
GO

CREATE INDEX IX_PopulationObservation_Indicator_Year
	ON dbo.PopulationObservation (IndicatorId, [Year]);
GO


