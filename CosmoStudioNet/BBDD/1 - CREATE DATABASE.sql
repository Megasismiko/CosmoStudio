/* 1) Crear BD limpia */
IF DB_ID(N'CosmoStudio') IS NOT NULL
BEGIN
    ALTER DATABASE CosmoStudio SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CosmoStudio;
END;
GO
CREATE DATABASE CosmoStudio;
GO
USE CosmoStudio;
GO

/* 2) Esquema */


/* 3) Tablas con BIGINT IDENTITY */

/* Proyectos */
CREATE TABLE Proyectos (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Tema NVARCHAR(400) NOT NULL,
    Origen NVARCHAR(20)  NOT NULL CONSTRAINT DF_Proyectos_Origen DEFAULT N'Manual', -- Manual|NASA|ESA|Otro
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Proyectos_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Proyectos PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Proyectos_Origen CHECK (Origen IN (N'Manual', N'NASA', N'ESA', N'Otro'))
);
CREATE INDEX IX_Proyectos_FechaCreacion ON Proyectos(FechaCreacion DESC);

/* Guiones (1:1 con Proyectos) */
CREATE TABLE Guiones (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    IdProyecto BIGINT NOT NULL,                           -- FK + UNIQUE para 1:1
    RutaOutline NVARCHAR(500) NOT NULL,
    RutaCompleto NVARCHAR(500) NOT NULL,
    Version INT NOT NULL CONSTRAINT DF_Guiones_Version DEFAULT 1,
    CONSTRAINT PK_Guiones PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Guiones_Proyectos FOREIGN KEY (IdProyecto) REFERENCES Proyectos(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX UQ_Guiones_Proyecto ON Guiones(IdProyecto);

/* Recursos (1:N con Proyectos) */
CREATE TABLE Recursos (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    IdProyecto BIGINT NOT NULL,
    Tipo NVARCHAR(20) NOT NULL,           -- Imagen|Musica|Voz|Otro
    Ruta NVARCHAR(500) NOT NULL,
    MetaJSON NVARCHAR(MAX) NULL,
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Recursos_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Recursos PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Recursos_Tipo CHECK (Tipo IN (N'Imagen', N'Musica', N'Voz', N'Otro')),
    CONSTRAINT FK_Recursos_Proyectos FOREIGN KEY (IdProyecto) REFERENCES Proyectos(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Recursos_Proyecto_Tipo ON Recursos(IdProyecto, Tipo);

/* TareasRender (1:N con Proyectos) */
CREATE TABLE TareasRender (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    IdProyecto BIGINT NOT NULL,
    Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_TareasRender_Estado DEFAULT N'EnCola',  -- EnCola|EnEjecucion|Error|Completado
    DuracionMinutos INT NOT NULL CONSTRAINT DF_TareasRender_Duracion DEFAULT 60,
    RutaVideoSalida NVARCHAR(500) NULL,
    RutasSalidaJSON NVARCHAR(MAX) NULL,
    FechaInicio DATETIME2 NULL,
    FechaFin DATETIME2 NULL,
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_TareasRender_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TareasRender PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_TareasRender_Estado CHECK (Estado IN (N'EnCola', N'EnEjecucion', N'Error', N'Completado')),
    CONSTRAINT CK_TareasRender_Duracion CHECK (DuracionMinutos BETWEEN 1 AND 600),
    CONSTRAINT FK_TareasRender_Proyectos FOREIGN KEY (IdProyecto) REFERENCES Proyectos(Id) ON DELETE CASCADE
);
CREATE INDEX IX_TareasRender_Proyecto_Estado ON TareasRender(IdProyecto, Estado);

/* Logs (N:1 con TareasRender) */
CREATE TABLE Logs (
    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Logs PRIMARY KEY,
    IdTareaRender BIGINT NOT NULL,
    Nivel NVARCHAR(10) NOT NULL CONSTRAINT CK_Logs_Nivel CHECK (Nivel IN (N'Info', N'Aviso', N'Error')),
    Mensaje NVARCHAR(MAX) NOT NULL,
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Logs_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Logs_TareasRender FOREIGN KEY (IdTareaRender) REFERENCES TareasRender(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Logs_Tarea_Fecha ON Logs(IdTareaRender, FechaCreacion);

/* 4) Vista */
IF OBJECT_ID('v_ResumenRender','V') IS NOT NULL
    DROP VIEW v_ResumenRender;
GO
CREATE VIEW v_ResumenRender AS
SELECT
    r.Id            AS IdTarea,
    r.IdProyecto,
    p.Titulo,
    p.Tema,
    r.Estado,
    r.DuracionMinutos,
    r.FechaCreacion,
    r.FechaInicio,
    r.FechaFin,
    r.RutaVideoSalida
FROM TareasRender r
JOIN Proyectos p ON p.Id = r.IdProyecto;
GO

/* 5) Datos de prueba (opcional) */
INSERT INTO Proyectos (Titulo, Tema, Origen)
VALUES (N'Piloto: Agujeros Negros', N'Agujeros negros y su papel en el universo', N'Manual');

INSERT INTO TareasRender (IdProyecto, Estado, DuracionMinutos)
SELECT TOP 1 Id, N'EnCola', 60 FROM Proyectos ORDER BY Id DESC;

SELECT TOP 5 * FROM Proyectos ORDER BY FechaCreacion DESC;
