-- Tabla para [BE-D09] Valoraciones y Reseñas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VALORACIONES_APP' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.VALORACIONES_APP (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TrayectoId INT NOT NULL,
        ConductorPerfilId INT NOT NULL,
        PasajeroUsuarioId INT NOT NULL,
        Puntuacion INT NOT NULL CHECK (Puntuacion BETWEEN 1 AND 5),
        Comentario NVARCHAR(500) NULL,
        Tags NVARCHAR(250) NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Valoracion_Trayecto_Pasajero UNIQUE (TrayectoId, PasajeroUsuarioId),
        CONSTRAINT FK_VALORACIONES_TRAYECTO FOREIGN KEY (TrayectoId) REFERENCES dbo.TRAYECTOS(Id) ON DELETE CASCADE,
        CONSTRAINT FK_VALORACIONES_CONDUCTOR FOREIGN KEY (ConductorPerfilId) REFERENCES dbo.PERFIL_APP(Id),
        CONSTRAINT FK_VALORACIONES_PASAJERO FOREIGN KEY (PasajeroUsuarioId) REFERENCES dbo.USUARIOSTAJAMAR(IDUSUARIO)
    );
END
GO

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'tajamar_mario_api')
BEGIN
    GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.VALORACIONES_APP TO [tajamar_mario_api];
END
GO
