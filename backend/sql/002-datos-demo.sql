-- Datos ficticios de BE-M05 para AZURETAJAMAR. Solo inserta registros marcados como demo.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @lockResult int;
EXEC @lockResult = sys.sp_getapplock @Resource = N'TajamarApp.BE-M05.Seed', @LockMode = 'Exclusive',
    @LockOwner = 'Transaction', @LockTimeout = 10000;
IF @lockResult < 0 THROW 50001, 'No se pudo obtener el bloqueo del seeder.', 1;

DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @role int = (
    SELECT MIN(IDROLE) FROM dbo.ROLESCHARLASTAJAMAR WHERE ROLE IN (N'ALUMNO', N'DemoMovilidad')
);
IF @role IS NULL
BEGIN
    SELECT @role = COALESCE(MAX(IDROLE), 0) + 1 FROM dbo.ROLESCHARLASTAJAMAR WITH (UPDLOCK, HOLDLOCK);
    INSERT dbo.ROLESCHARLASTAJAMAR (IDROLE, ROLE) VALUES (@role, N'DemoMovilidad');
END;

DECLARE @people table (Nombre nvarchar(70), Email nvarchar(70) PRIMARY KEY, Curso nvarchar(150));
INSERT @people VALUES
    (N'Ana', N'bem05.ana@example.invalid', N'[DEMO BE-M05] Desarrollo Full Stack'),
    (N'Bruno', N'bem05.bruno@example.invalid', N'[DEMO BE-M05] Desarrollo Full Stack'),
    (N'Carla', N'bem05.carla@example.invalid', N'[DEMO BE-M05] Cloud Computing'),
    (N'Daniel', N'bem05.daniel@example.invalid', N'[DEMO BE-M05] Cloud Computing');

INSERT dbo.CURSOSTAJAMAR (NOMBRE, FECHAINICIO, FECHAFIN, ACTIVO)
SELECT DISTINCT p.Curso, CAST(@now AS date), DATEADD(month, 3, CAST(@now AS date)), 0
FROM @people p
WHERE NOT EXISTS (SELECT 1 FROM dbo.CURSOSTAJAMAR c WHERE c.NOMBRE = p.Curso);

INSERT dbo.USUARIOSTAJAMAR (NOMBRE, APELLIDOS, EMAIL, ESTADO, IDROLE)
SELECT p.Nombre, N'Demo BE-M05', p.Email, 0, @role FROM @people p
WHERE NOT EXISTS (SELECT 1 FROM dbo.USUARIOSTAJAMAR u WHERE u.EMAIL = p.Email);

INSERT dbo.CURSOSUSUARIOSTAJAMAR (IDCURSO, IDUSUARIO)
SELECT curso.IDCURSO, u.IDUSUARIO
FROM @people p
JOIN dbo.USUARIOSTAJAMAR u ON u.EMAIL = p.Email AND u.APELLIDOS = N'Demo BE-M05'
CROSS APPLY (SELECT TOP (1) c.IDCURSO FROM dbo.CURSOSTAJAMAR c WHERE c.NOMBRE = p.Curso ORDER BY c.IDCURSO) curso
WHERE NOT EXISTS (SELECT 1 FROM dbo.CURSOSUSUARIOSTAJAMAR cu
                  WHERE cu.IDCURSO = curso.IDCURSO AND cu.IDUSUARIO = u.IDUSUARIO);

DECLARE @organizer int = (SELECT IDUSUARIO FROM dbo.USUARIOSTAJAMAR
    WHERE EMAIL = N'bem05.ana@example.invalid' AND APELLIDOS = N'Demo BE-M05');
IF @organizer IS NULL THROW 50002, 'El correo del organizador demo está ocupado.', 1;

INSERT dbo.EVENTOS_APP (Titulo, Descripcion, FechaHora, Ubicacion, CreadorId, FechaCreacion)
SELECT v.Titulo, N'Evento ficticio para verificar BE-M05.', DATEADD(day, v.Dias, @now), N'Tajamar', @organizer, @now
FROM (VALUES (N'[DEMO BE-M05] Charla de tecnología', 7), (N'[DEMO BE-M05] Torneo Tajamar', 14),
             (N'[DEMO BE-M05] Encuentro de empresas', 21)) v(Titulo, Dias)
WHERE NOT EXISTS (SELECT 1 FROM dbo.EVENTOS_APP e WHERE e.CreadorId = @organizer AND e.Titulo = v.Titulo);

IF NOT EXISTS (SELECT 1 FROM dbo.PERFIL_APP WHERE IdUsuarioTajamar = @organizer)
    INSERT dbo.PERFIL_APP (IdUsuarioTajamar, EsConductor, PlazasCoche, FechaCreacion)
    VALUES (@organizer, 1, 1, @now);

COMMIT TRANSACTION;
