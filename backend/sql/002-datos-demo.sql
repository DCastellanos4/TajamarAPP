-- Ejecutar solo en una base de desarrollo creada para el módulo.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'movilidad.CursoDemo', N'U') IS NULL
    CREATE TABLE movilidad.CursoDemo (Id int PRIMARY KEY, Nombre nvarchar(100) NOT NULL);
IF OBJECT_ID(N'movilidad.UsuarioDemo', N'U') IS NULL
    CREATE TABLE movilidad.UsuarioDemo (
        Id int PRIMARY KEY, Nombre nvarchar(100) NOT NULL, Email nvarchar(150) NOT NULL,
        CursoId int NOT NULL REFERENCES movilidad.CursoDemo(Id)
    );

INSERT movilidad.CursoDemo (Id, Nombre)
SELECT v.Id, v.Nombre FROM (VALUES (1, N'Desarrollo Full Stack'), (2, N'Cloud Computing')) v(Id, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM movilidad.CursoDemo c WHERE c.Id = v.Id);

INSERT movilidad.UsuarioDemo (Id, Nombre, Email, CursoId)
SELECT v.Id, v.Nombre, v.Email, v.CursoId FROM (VALUES
    (900001, N'Ana Demo', N'ana@example.test', 1),
    (900002, N'Bruno Demo', N'bruno@example.test', 1),
    (900003, N'Carla Demo', N'carla@example.test', 2),
    (900004, N'Daniel Demo', N'daniel@example.test', 2)
) v(Id, Nombre, Email, CursoId)
WHERE NOT EXISTS (SELECT 1 FROM movilidad.UsuarioDemo u WHERE u.Id = v.Id);

INSERT movilidad.Evento (Nombre, Inicio, Lugar, OrganizadorId, OrganizadorNombre)
SELECT v.Nombre, DATEADD(day, v.Dias, SYSDATETIMEOFFSET()), N'Tajamar', 900001, N'Ana Demo'
FROM (VALUES (N'Charla de tecnología', 7), (N'Torneo Tajamar', 14), (N'Encuentro de empresas', 21)) v(Nombre, Dias)
WHERE NOT EXISTS (SELECT 1 FROM movilidad.Evento e WHERE e.OrganizadorId = 900001 AND e.Nombre = v.Nombre);

IF NOT EXISTS (SELECT 1 FROM movilidad.Trayecto WHERE ConductorId = 900001)
    INSERT movilidad.Trayecto (ConductorId, Salida, Capacidad, PlazasDisponibles)
    VALUES (900001, DATEADD(day, 2, SYSDATETIMEOFFSET()), 1, 1);

COMMIT;
