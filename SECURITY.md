# Política de seguridad

## Mantenedor

| Rol | Nombre | GitHub |
|-----|--------|--------|
| Mantenedor principal | Santiago Caicedo | [@santyxswc](https://github.com/santyxswc) |

Santiago Caicedo es el autor y mantenedor principal de FarmaciaApp y quien recibe, evalúa y corrige los reportes de seguridad.

## Versiones soportadas

| Versión | Soporte |
|---------|---------|
| 2.2.x (rama `master` / última release) | Sí |
| < 2.2 | No |

## Reportar una vulnerabilidad

Si encuentras un problema de seguridad en la aplicación de escritorio, la API REST o la interfaz web, **no** abras un issue público con los detalles.

- Usa el reporte privado de GitHub: [**Report a vulnerability**](https://github.com/santyxswc/FarmaciaApp/security/advisories/new), o
- escribe directamente al mantenedor principal, [@santyxswc](https://github.com/santyxswc), por un canal privado.

Incluye, si es posible: pasos para reproducir, componente afectado (escritorio, API o web), impacto estimado y versión.

### Qué esperar

- Confirmación de recibido en un máximo de **72 horas**.
- Una primera evaluación (si se acepta y su gravedad) en un máximo de **7 días**.
- Las correcciones se publican en una nueva release y, cuando aplica, con un aviso de seguridad (GitHub Security Advisory) que da crédito a quien reportó, si así lo desea.

## Alcance

Son de especial interés los reportes sobre:

- Autenticación y sesiones de la API (tokens JWT, roles y permisos).
- Almacenamiento de contraseñas (PBKDF2-SHA256).
- Inyección SQL u otros accesos indebidos a la base de datos Oracle.
- XSS o saltos de la política de contenido (`Content-Security-Policy`) en la interfaz web.

## Secretos

- No incluyas cadenas de conexión, contraseñas ni llaves de firma JWT en issues, PRs ni mensajes.
- Los valores reales van en variables de entorno (ver `.env.example`), nunca en el repositorio.
- Si publicas una credencial por error, rótala de inmediato.
