#!/usr/bin/env bash
# Carga database/schema.sql solo si la base de datos está vacía.
# Lo ejecuta el servicio "db-init" de docker-compose.yml; es seguro correrlo varias veces.
set -euo pipefail

CONEXION="${APP_USER}/${APP_USER_PASSWORD}@//${DB_HOST:-oracle}:1521/FREEPDB1"

existe=$(printf 'SET HEADING OFF FEEDBACK OFF PAGESIZE 0\nSELECT COUNT(*) FROM user_tables WHERE table_name = '"'"'TBL_USUARIO'"'"';\nEXIT\n' \
  | sqlplus -s "$CONEXION" | tr -d '[:space:]')

if [ "$existe" = "0" ]; then
  echo "Base de datos vacía: cargando schema.sql"
  printf '@/database/schema.sql\nEXIT\n' | sqlplus -s "$CONEXION"
else
  echo "El esquema ya existe: no se toca"
fi
