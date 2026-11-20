SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'movilidad') IS NULL
    EXEC(N'CREATE SCHEMA movilidad');

CREATE TABLE movilidad.Evento (
    Id int IDENTITY PRIMARY KEY,
    Nombre nvarchar(150) NOT NULL,
    Inicio datetimeoffset NOT NULL,
    Lugar nvarchar(250) NOT NULL,
    Imagen nvarchar(2048) NULL,
    OrganizadorId int NOT NULL,
    OrganizadorNombre nvarchar(150) NOT NULL,
    Activo bit NOT NULL CONSTRAINT DF_MovilidadEvento_Activo DEFAULT (1)
);
CREATE INDEX IX_MovilidadEvento_Activo_Inicio ON movilidad.Evento (Activo, Inicio, Id);

CREATE TABLE movilidad.Asistencia (
    EventoId int NOT NULL REFERENCES movilidad.Evento(Id),
    UsuarioId int NOT NULL,
    UsuarioNombre nvarchar(150) NOT NULL,
    CONSTRAINT PK_MovilidadAsistencia PRIMARY KEY (EventoId, UsuarioId)
);

CREATE TABLE movilidad.Trayecto (
    Id int IDENTITY PRIMARY KEY,
    ConductorId int NOT NULL,
    EventoId int NULL REFERENCES movilidad.Evento(Id),
    Salida datetimeoffset NOT NULL,
    Capacidad int NOT NULL,
    PlazasDisponibles int NOT NULL,
    Activo bit NOT NULL CONSTRAINT DF_MovilidadTrayecto_Activo DEFAULT (1),
    CONSTRAINT CK_MovilidadTrayecto_Plazas CHECK (Capacidad > 0 AND PlazasDisponibles BETWEEN 0 AND Capacidad)
);

CREATE TABLE movilidad.Reserva (
    Id bigint IDENTITY PRIMARY KEY,
    TrayectoId int NOT NULL REFERENCES movilidad.Trayecto(Id),
    UsuarioId int NOT NULL,
    Estado nvarchar(15) NOT NULL CONSTRAINT DF_MovilidadReserva_Estado DEFAULT (N'Confirmada'),
    CONSTRAINT CK_MovilidadReserva_Estado CHECK (Estado IN (N'Confirmada', N'Cancelada'))
);
CREATE UNIQUE INDEX UX_MovilidadReserva_Confirmada ON movilidad.Reserva (TrayectoId, UsuarioId)
    WHERE Estado = N'Confirmada';

CREATE TABLE movilidad.ColaEspera (
    Id bigint IDENTITY PRIMARY KEY,
    TrayectoId int NOT NULL REFERENCES movilidad.Trayecto(Id),
    UsuarioId int NOT NULL,
    Posicion int NOT NULL CONSTRAINT CK_MovilidadCola_Posicion CHECK (Posicion > 0),
    Estado nvarchar(15) NOT NULL CONSTRAINT DF_MovilidadCola_Estado DEFAULT (N'EnEspera'),
    CONSTRAINT CK_MovilidadCola_Estado CHECK (Estado IN (N'EnEspera', N'Promocionado'))
);
CREATE UNIQUE INDEX UX_MovilidadCola_EnEspera ON movilidad.ColaEspera (TrayectoId, UsuarioId)
    WHERE Estado = N'EnEspera';
CREATE INDEX IX_MovilidadCola_Posicion ON movilidad.ColaEspera (TrayectoId, Estado, Posicion, Id)
    INCLUDE (UsuarioId);

COMMIT;
