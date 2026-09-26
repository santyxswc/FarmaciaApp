# FarmaciaApp

Sistema de escritorio para la gestión integral de operaciones de farmacia (inventario, ventas, clientes, proveedores, promociones y reclamos), desarrollado en **.NET 10 (WPF)** con arquitectura en capas (**MVVM**), acceso a datos mediante **Dapper** y base de datos relacional **Oracle**.

---

## Arquitectura de la Solución

```
FarmaciaApp/
├── FarmaciaApp.Core/          # Capa de dominio, entidades y acceso a datos
│   ├── Database/              # Conexión Oracle (OracleConnection, DbConfig)
│   ├── Models/                # Entidades de dominio (Cliente, Factura, Producto...)
│   ├── Repository/            # Repositorios SQL implementados con Dapper
│   └── Service/               # Servicios de lógica de negocio y validaciones
├── FarmaciaApp.UI/            # Capa de presentación WPF (.NET 10 Desktop)
│   ├── ViewModels/            # ViewModels MVVM con CommunityToolkit.Mvvm
│   ├── Views/                 # Vistas XAML (Productos, Clientes, Facturas...)
│   ├── Helpers/               # Servicios de navegación y utilidades de UI
│   └── appsettings.example.json # Plantilla de configuración de conexión
└── database/
    └── schema.sql             # Script DDL y datos semilla para Oracle
```

---

## Base de Datos (Oracle)

El esquema de la base de datos se encuentra completamente definido en [`database/schema.sql`](database/schema.sql). Incluye la definición de las **10 tablas principales**, **3 tablas intermedias/detalle** y las **3 secuencias** utilizadas por el sistema.

### Tablas Principales (10)

| # | Tabla | Descripción | Llave Primaria |
|---|---|---|---|
| 1 | `TBL_PERSONA` | Datos personales base (nombre, apellido, dirección, teléfono, email) | `PER_ID` (Secuencia `SEQ_PERSONA`) |
| 2 | `TBL_CLIENTE` | Registro de clientes del sistema (extiende Persona) | `CLI_ID` (Identity / FK a `TBL_PERSONA`) |
| 3 | `TBL_VENDEDOR` | Empleados / vendedores de la farmacia (extiende Persona) | `VEN_ID` (Identity / FK a `TBL_PERSONA`) |
| 4 | `TBL_PRODUCTO` | Catálogo de medicamentos y artículos farmacéuticos | `PRO_ID` (Secuencia `SEQ_PRODUCTO`) |
| 5 | `TBL_PROVEEDOR` | Empresas proveedoras y sus contactos | `PRO_ID` |
| 6 | `TBL_PROMOCION` | Campañas promocionales, descuentos y vigencias | `PRM_ID` |
| 7 | `TBL_PAGO` | Métodos de pago y registros de transacción | `PAG_ID` |
| 8 | `TBL_FACTURA` | Facturación de compras vinculada a cliente, vendedor y pago | `FAC_NUM_FACTURA` |
| 9 | `TBL_RECLAMO` | Peticiones y reclamos asociados a facturas | `REC_ID_RECLAMO` (Secuencia `SEQ_RECLAMO`) |
| 10 | `TBL_REINTEGRO` | Devoluciones y compensaciones monetarias de reclamos | `REI_ID` |

### Tablas de Asociación y Detalle (3)

| Tabla | Descripción | Llaves / Relaciones |
|---|---|---|
| `PROVEE_PRODUC` | Relación muchos a muchos entre Proveedores y Productos | `(PROV_ID, PRO_ID)` |
| `PROMO_PRODU` | Relación de productos vinculados a promociones activas | `(PRM_ID, PRO_ID)` |
| `FACTU_PRODUC` | Detalle de líneas de producto facturadas (cantidad, precio, subtotal) | `(FAC_NUM_FACTURA, PRO_ID)` |

### Secuencias (3)

| Secuencia | Uso |
|---|---|
| `SEQ_PERSONA` | Generación secuencial de identificadores para `TBL_PERSONA` |
| `SEQ_PRODUCTO` | Generación secuencial de códigos de producto en `TBL_PRODUCTO` |
| `SEQ_RECLAMO` | Generación secuencial de radicados en `TBL_RECLAMO` |

---

## Configuración y Puesta en Marcha

### 1. Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Motor Oracle Database 19c / 21c / 23c / XE (o contenedor Docker con Oracle Express)
- Visual Studio 2022 / 2025 o VS Code con C# Dev Kit

### 2. Configurar Base de Datos

Ejecute el script SQL en su base de datos Oracle (a través de SQL*Plus, SQL Developer o DBeaver):

```sql
@database/schema.sql
```

El script crea automáticamente todas las secuencias, tablas, restricciones de integridad referencial y un conjunto de datos iniciales de prueba.

> **¿Ya tenías la base creada con una versión anterior del script?** Ejecuta una vez
> `@database/migracion_ids.sql` (Oracle 18c+). Sincroniza las secuencias y las identidades de
> `TBL_CLIENTE` / `TBL_VENDEDOR` con los datos existentes; sin esto, crear clientes desde la app
> falla con `ORA-00001`.

### Funcionalidades

- **Ventas (Facturas → Nueva Factura):** cliente, vendedor, método de pago y productos. En una sola transacción
  registra el pago, la factura y sus líneas y descuenta el stock; si algún producto no alcanza, no se guarda nada.
  - Los precios incluyen IVA (19 %): subtotal = total / 1.19.
  - Se aplica automáticamente el mayor descuento de las promociones vigentes del producto (`PROMO_PRODU`).
- **Detalle de factura:** botón "Ver Detalle" o doble clic en la factura.
- **Vendedores:** en Personas, la casilla "Es vendedor" habilita a la persona para registrar ventas;
  la lista muestra el rol de cada una (Cliente, Vendedor o ambos). No se puede quitar el rol a quien ya tiene facturas.
- **Reclamos:** crear (asociado a una factura) y editar descripción y estado (Pendiente, En proceso, Resuelto, Rechazado).
- **Integridad de datos:** no se pueden eliminar clientes, vendedores ni productos que aparecen en facturas;
  la app muestra el motivo en lugar del error de Oracle.

### 3. Configurar Cadena de Conexión

Copie la plantilla de configuración:

```bash
cp FarmaciaApp.UI/appsettings.example.json FarmaciaApp.UI/appsettings.json
```

Edite `FarmaciaApp.UI/appsettings.json` ajustando su usuario, contraseña y host de Oracle:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=farmacia;Password=TU_PASSWORD_LOCAL;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));"
  }
}
```

> **Nota de Seguridad**: `appsettings.json` está incluido en `.gitignore` para evitar la subida accidental de credenciales sensibles al control de versiones.

### 4. Compilar y Ejecutar

```bash
dotnet build
dotnet run --project FarmaciaApp.UI/FarmaciaApp.UI.csproj
```

### 5. Ejecutar en Linux / macOS (Avalonia)

`FarmaciaApp.UI` es WPF y solo corre en Windows. `FarmaciaApp.Desktop` es la misma app portada a
[Avalonia](https://avaloniaui.net/) (reutiliza `FarmaciaApp.Core`) y corre en Linux, macOS y Windows.

```bash
# 1. Base de datos Oracle Free en Docker
docker compose up -d
docker compose ps        # esperar a que el estado sea "healthy"
# Sin el plugin compose, el equivalente es:
#   docker run -d --name farmacia-oracle -p 1521:1521 \
#     -e ORACLE_PASSWORD=admin123 -e APP_USER=farmacia -e APP_USER_PASSWORD=farmacia123 -e TZ=America/Bogota \
#     -v "$PWD/database:/database:ro" -v farmacia-oracle-data:/opt/oracle/oradata \
#     gvenzl/oracle-free:23-slim

# 2. Cargar el esquema y los datos de prueba
printf '@/database/schema.sql\nEXIT\n' | docker exec -i farmacia-oracle sqlplus -s farmacia/farmacia123@//localhost/FREEPDB1

# 3. Configuración (Oracle Free usa el servicio FREEPDB1)
cp FarmaciaApp.Desktop/appsettings.example.json FarmaciaApp.Desktop/appsettings.json
#   -> Password=farmacia123 y SERVICE_NAME=FREEPDB1

# 4. Ejecutar
dotnet run --project FarmaciaApp.Desktop
```

**Credenciales por defecto en UI:**
- Usuario: `admin`
- Contraseña: `prueba`
