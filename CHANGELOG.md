# Cambios

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y versionado semántico.

## [Sin publicar]

### Agregado
- Flujo de release: al subir una etiqueta `v*` se generan los ejecutables de Windows y Linux y se crea el release en GitHub.
- CI con caché de NuGet, revisión de paquetes vulnerables y validación de `schema.sql` contra Oracle.
- Análisis estático con CodeQL y actualizaciones automáticas con Dependabot.
- `Makefile` con los comandos habituales y variables de entorno para `docker-compose.yml`.

## [2.1.0]

### Cambiado
- Arquitectura por capas (Core, Infrastructure, Desktop) con inyección de dependencias.
- Pruebas unitarias de los servicios con repositorios simulados.
