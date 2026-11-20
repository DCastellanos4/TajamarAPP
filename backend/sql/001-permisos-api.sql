-- Ejecutar después de crear la cuenta contenida tajamar_mario_api.
GRANT SELECT ON OBJECT::dbo.EVENTOS_APP TO [tajamar_mario_api];
GRANT INSERT, UPDATE ON OBJECT::dbo.EVENTOS_APP TO [tajamar_mario_api];
GRANT SELECT, INSERT, DELETE ON OBJECT::dbo.EVENTO_ASISTENTES TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.TRAYECTOS TO [tajamar_mario_api];
GRANT UPDATE ON OBJECT::dbo.TRAYECTOS (PlazasDisponibles, Estado) TO [tajamar_mario_api];
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.RESERVAS_TRAYECTO TO [tajamar_mario_api];
GRANT SELECT, INSERT, UPDATE ON OBJECT::dbo.COLA_ESPERA TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.PERFIL_APP TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.USUARIOSTAJAMAR (IDUSUARIO, NOMBRE, APELLIDOS, EMAIL, ESTADO, IMAGEN, IDROLE) TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.ROLESCHARLASTAJAMAR TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.CURSOSTAJAMAR TO [tajamar_mario_api];
GRANT SELECT ON OBJECT::dbo.CURSOSUSUARIOSTAJAMAR TO [tajamar_mario_api];
