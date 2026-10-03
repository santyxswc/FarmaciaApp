# Cambios

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y versionado semántico.

## [Sin publicar]

## [2.2.0] - 2026-10-02

### Agregado
- Cobertura de pruebas (`make cobertura`) y su resumen en cada ejecución de la CI.
- Verificación de formato con `dotnet format` en la CI (`make formato` para aplicarlo).
- Interfaz web (HTML, CSS y JavaScript sin dependencias) servida por la API: login, productos, nueva venta, facturas, clientes, reclamos, promociones, personas, reportes, movimientos y usuarios, con cambio de la propia contraseña.
- Cabeceras de seguridad (`Content-Security-Policy`, `X-Content-Type-Options`, `Referrer-Policy`) y `GET /api/ventas/metodos-pago`.
- Endpoints de la API para clientes, personas, proveedores, promociones, reclamos, facturas y ventas, reportes, movimientos y usuarios.
- Límite de intentos de login configurable (`RateLimit:LoginPorMinuto`).
- API REST (`FarmaciaApp.Api`) con autenticación JWT, rutas de autenticación y productos, documentación OpenAPI con Scalar, health checks y límite de intentos de login.
- Pruebas de integración de la API.
- `Dockerfile` de la API, carga automática del esquema en Docker Compose y publicación de la imagen en GitHub Container Registry.
- Licencia MIT.
- Flujo de release: al subir una etiqueta `v*` se generan los ejecutables de Windows y Linux y se crea el release en GitHub.
- CI con caché de NuGet, revisión de paquetes vulnerables y prueba del stack completo con Docker Compose.
- Análisis estático con CodeQL y actualizaciones automáticas con Dependabot.
- `Makefile` con los comandos habituales y variables de entorno para `docker-compose.yml`.

### Cambiado
- El registro de repositorios pasó a `FarmaciaApp.Infrastructure` para compartirlo entre el escritorio y la API.
- Iniciar sesión con una cuenta desactivada lanza `CuentaDesactivadaException` (sigue siendo un `InvalidOperationException`).

## [2.1.0]

### Cambiado
- Arquitectura por capas (Core, Infrastructure, Desktop) con inyección de dependencias.
- Pruebas unitarias de los servicios con repositorios simulados.
