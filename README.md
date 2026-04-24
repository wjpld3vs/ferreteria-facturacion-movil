# FerreteriaInventarioApp

Aplicacion hibrida multiplataforma para una pequena ferreteria local. La solucion separa el backend en una `ASP.NET Core Web API` y el frontend en una app `.NET MAUI`, permitiendo gestionar inventario, proveedores, clientes, compras, ventas, stock y usuarios con autenticacion por roles.

## Analisis funcional del sistema

### Problema que resuelve

Una ferreteria pequena suele llevar inventario, compras y ventas en cuadernos, hojas sueltas o archivos dispersos. Eso provoca:

- errores al calcular existencias;
- ventas con stock desactualizado;
- poca trazabilidad de compras y ventas;
- dificultad para separar permisos entre administrador y operario;
- poca visibilidad del inventario de bajo stock.

### Objetivos funcionales

- Gestionar productos, clientes, proveedores y usuarios.
- Registrar compras y ventas con detalle.
- Actualizar stock automaticamente.
- Diferenciar permisos entre `Admin` y `Operario`.
- Consultar reportes basicos de inventario, stock bajo, compras y ventas por fecha.
- Consumir una API desde una app MAUI con vistas XAML y patron MVVM.

### Objetivos tecnicos

- Mantener una arquitectura con responsabilidades separadas.
- Centralizar reglas criticas en backend.
- Usar DTOs para no exponer entidades.
- Trabajar con `SQLite + EF Core`.
- Usar `JWT` para autenticacion y `SecureStorage` para persistencia segura del token.

## Version de .NET elegida

Se eligio **.NET 9** como objetivo de la solucion.

### Justificacion

- Tiene soporte en `Visual Studio 2022` reciente, especialmente 17.12 o superior.
- Se alinea mejor con el workload MAUI disponible en el entorno de desarrollo validado.
- Permite mantener API y MAUI sobre una misma generacion de framework moderna.
- Sigue siendo una opcion valida para una solucion nueva en Visual Studio 2022 actualizada.

## Tecnologias usadas

- Visual Studio 2022
- .NET 9
- ASP.NET Core Web API
- .NET MAUI
- C#
- XAML
- SQLite
- Entity Framework Core
- Swagger / OpenAPI
- JWT Bearer Authentication
- MVVM
- HttpClient
- SecureStorage
- Inyeccion de dependencias

## Arquitectura

La solucion contiene exactamente dos proyectos principales:

- `FerreteriaInventario.Api`
- `FerreteriaInventario.Maui`

### Diagrama textual

```text
.NET MAUI App (XAML + MVVM)
        |
        | HttpClient + Bearer Token
        v
ASP.NET Core Web API
        |
        | EF Core
        v
SQLite
```

### Responsabilidades por proyecto

#### FerreteriaInventario.Api

- exponer endpoints REST;
- validar reglas de negocio;
- autenticar y autorizar usuarios;
- calcular totales de compras y ventas;
- controlar stock;
- persistir datos en SQLite;
- publicar Swagger/OpenAPI;
- inicializar datos de prueba.

#### FerreteriaInventario.Maui

- iniciar sesion;
- almacenar token en `SecureStorage`;
- consumir endpoints protegidos;
- mostrar vistas XAML;
- mantener estado mediante ViewModels;
- filtrar menu segun rol;
- registrar ventas y compras;
- mostrar mensajes amigables y errores de conectividad.

## Estructura de carpetas

```text
FerreteriaInventarioApp.sln
FerreteriaInventario.Api/
  Controllers/
  Data/
  DTOs/
  Helpers/
  Interfaces/
  Migrations/
  Models/
  Seed/
  Services/
  Program.cs
  appsettings.json
  appsettings.example.json
FerreteriaInventario.Maui/
  Helpers/
  Models/
  Services/
  ViewModels/
  Views/
  Resources/
  App.xaml
  AppShell.xaml
  MauiProgram.cs
```

## Modelo de datos

### Entidades principales

- `Rol`
- `Usuario`
- `Producto`
- `Cliente`
- `Proveedor`
- `Compra`
- `DetalleCompra`
- `Venta`
- `DetalleVenta`

### Relaciones

- Un `Rol` tiene muchos `Usuarios`.
- Un `Proveedor` tiene muchas `Compras`.
- Un `Cliente` tiene muchas `Ventas`.
- Una `Compra` tiene muchos `DetalleCompra`.
- Una `Venta` tiene muchos `DetalleVenta`.
- Un `Producto` participa en muchos detalles de compra y venta.

### Reglas y restricciones de datos

- indice unico para `Producto.Codigo`;
- indice unico para `Usuario.NombreUsuario`;
- indice unico para `Usuario.Email`;
- indice unico para `Cliente.Documento`;
- indice unico para `Proveedor.DocumentoFiscal`;
- precision monetaria para subtotales y totales;
- relaciones `Restrict` en catalogos clave para evitar borrados accidentales.

## Reglas de negocio implementadas

1. Solo usuarios activos pueden iniciar sesion.
2. Las contrasenas se almacenan con `PasswordHasher`.
3. `Admin` puede gestionar catalogos, usuarios, compras, ventas y reportes.
4. `Operario` puede iniciar sesion, consultar productos y clientes, y registrar ventas.
5. Una compra incrementa stock.
6. Una venta disminuye stock.
7. No se permite stock negativo.
8. No se permiten compras sin detalles.
9. No se permiten ventas sin detalles.
10. Los totales se recalculan en backend.
11. El frontend solo muestra calculos preliminares.
12. El codigo de producto debe ser unico.
13. Los productos inactivos no se pueden vender.
14. Clientes, proveedores y productos se desactivan en vez de eliminarse fisicamente.
15. Las respuestas de error usan un formato consistente.

## Seguridad

- autenticacion con `JWT`;
- autorizacion por roles con `[Authorize(Roles = "...")]`;
- `PasswordHasher<Usuario>` para hashing seguro;
- token Bearer enviado en cada solicitud protegida;
- token y usuario guardados en `SecureStorage` en MAUI;
- manejo de `401` y `403` con cierre de sesion y retorno a login.

## Datos de prueba

El seed inserta:

- roles: `Admin`, `Operario`;
- usuarios:
  - `admin / Admin123*`
  - `operario / Operario123*`
- al menos 15 productos tipicos de ferreteria;
- 5 clientes;
- 5 proveedores;
- 3 compras con detalles;
- 3 ventas con detalles.

Estas credenciales son **solo para desarrollo** y deben cambiarse en produccion.

## Configuracion de la API

### Archivos clave

- `FerreteriaInventario.Api/appsettings.json`
- `FerreteriaInventario.Api/appsettings.example.json`
- `FerreteriaInventario.Api/Properties/launchSettings.json`

### Conexion SQLite

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=ferreteria_inventario.db"
}
```

### JWT

```json
"JwtSettings": {
  "Key": "FerreteriaInventarioJwtKey_Desarrollo_SoloCambiarEnProduccion_2026",
  "Issuer": "FerreteriaInventario.Api",
  "Audience": "FerreteriaInventario.Maui",
  "ExpirationMinutes": 480
}
```

### Swagger/OpenAPI

- URL HTTP desarrollo: `http://localhost:5099/swagger`
- URL HTTPS desarrollo: `https://localhost:7099/swagger`

## Configuracion de MAUI

### BaseUrl centralizada

Archivo:

- `FerreteriaInventario.Maui/Helpers/ApiSettings.cs`

Valores actuales:

- Windows local: `http://localhost:5099/`
- Android Emulator: `http://10.0.2.2:5099/`
- Dispositivo fisico: `http://192.168.1.100:5099/`

### Recomendaciones por entorno

- En Windows, ejecutar la API en `http://localhost:5099`.
- En Android Emulator, usar `10.0.2.2` como alias del host.
- En dispositivo fisico, reemplazar `PhysicalDeviceBaseUrl` por la IP real del equipo en la red local.

### Token

- Se guarda con `SecureStorage`.
- Si la API responde `401` o `403`, la app invalida la sesion y redirige al login.

## Endpoints principales

### Auth

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| POST | `/api/auth/login` | Publico | `LoginRequestDto` | `LoginResponseDto` | 200, 401, 403 |
| POST | `/api/auth/register` | Admin | `UsuarioCreateDto` | `UsuarioResponseDto` | 201, 400, 403 |
| GET | `/api/auth/me` | Autenticado | - | `UsuarioResponseDto` | 200, 401 |

Ejemplo `POST /api/auth/login`

```json
{
  "usuarioOEmail": "admin",
  "password": "Admin123*"
}
```

```json
{
  "token": "jwt-token",
  "expiration": "2026-04-24T18:00:00Z",
  "usuario": {
    "id": 1,
    "nombre": "Administrador General",
    "nombreUsuario": "admin",
    "email": "admin@ferreteria.local",
    "rolId": 1,
    "rolNombre": "Admin",
    "activo": true,
    "fechaCreacion": "2026-04-24T12:00:00Z"
  }
}
```

Validaciones:

- usuario y contrasena obligatorios;
- usuario debe existir;
- usuario debe estar activo;
- contrasena valida.

### Usuarios

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/usuarios` | Admin | - | `List<UsuarioResponseDto>` | 200, 401, 403 |
| GET | `/api/usuarios/{id}` | Admin | - | `UsuarioResponseDto` | 200, 404 |
| POST | `/api/usuarios` | Admin | `UsuarioCreateDto` | `UsuarioResponseDto` | 201, 400 |
| PUT | `/api/usuarios/{id}` | Admin | `UsuarioUpdateDto` | `UsuarioResponseDto` | 200, 400, 404 |
| PATCH | `/api/usuarios/{id}/activar` | Admin | - | `UsuarioResponseDto` | 200, 404 |
| PATCH | `/api/usuarios/{id}/desactivar` | Admin | - | `UsuarioResponseDto` | 200, 404 |

Ejemplo `POST /api/usuarios`

```json
{
  "nombre": "Vendedor 1",
  "nombreUsuario": "vendedor1",
  "email": "vendedor1@ferreteria.local",
  "password": "Temporal123*",
  "rolId": 2,
  "activo": true
}
```

Validaciones:

- `NombreUsuario` unico;
- `Email` unico;
- `RolId` existente;
- password minima de 8 caracteres.

### Roles

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/roles` | Admin | - | `List<RolResponseDto>` | 200, 401, 403 |

### Productos

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/productos` | Admin, Operario | - | `List<ProductoResponseDto>` | 200 |
| GET | `/api/productos/{id}` | Admin, Operario | - | `ProductoResponseDto` | 200, 404 |
| GET | `/api/productos/buscar?texto=` | Admin, Operario | Query | `List<ProductoResponseDto>` | 200 |
| GET | `/api/productos/stock-bajo` | Admin, Operario | - | `List<StockBajoDto>` | 200 |
| POST | `/api/productos` | Admin | `ProductoCreateDto` | `ProductoResponseDto` | 201, 400 |
| PUT | `/api/productos/{id}` | Admin | `ProductoUpdateDto` | `ProductoResponseDto` | 200, 400, 404 |
| PATCH | `/api/productos/{id}/activar` | Admin | - | `ProductoResponseDto` | 200, 404 |
| PATCH | `/api/productos/{id}/desactivar` | Admin | - | `ProductoResponseDto` | 200, 404 |

Ejemplo `POST /api/productos`

```json
{
  "codigo": "FER-100",
  "nombre": "Sierra manual",
  "descripcion": "Sierra para madera",
  "categoria": "Herramientas",
  "marca": "Truper",
  "unidadMedida": "Unidad",
  "precioCompra": 9.50,
  "precioVenta": 13.50,
  "stockActual": 6,
  "stockMinimo": 2,
  "activo": true
}
```

Validaciones:

- codigo unico;
- precio venta >= precio compra;
- stock >= 0;
- stock minimo >= 0.

### Clientes

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/clientes` | Admin, Operario | - | `List<ClienteResponseDto>` | 200 |
| GET | `/api/clientes/{id}` | Admin, Operario | - | `ClienteResponseDto` | 200, 404 |
| POST | `/api/clientes` | Admin | `ClienteCreateDto` | `ClienteResponseDto` | 201, 400 |
| PUT | `/api/clientes/{id}` | Admin | `ClienteUpdateDto` | `ClienteResponseDto` | 200, 400, 404 |
| PATCH | `/api/clientes/{id}/desactivar` | Admin | - | `ClienteResponseDto` | 200, 404 |

Ejemplo `POST /api/clientes`

```json
{
  "nombre": "Cliente de prueba",
  "documento": "CL-100",
  "telefono": "8888-9999",
  "email": "cliente@local.test",
  "direccion": "Managua",
  "activo": true
}
```

### Proveedores

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/proveedores` | Admin | - | `List<ProveedorResponseDto>` | 200 |
| GET | `/api/proveedores/{id}` | Admin | - | `ProveedorResponseDto` | 200, 404 |
| POST | `/api/proveedores` | Admin | `ProveedorCreateDto` | `ProveedorResponseDto` | 201, 400 |
| PUT | `/api/proveedores/{id}` | Admin | `ProveedorUpdateDto` | `ProveedorResponseDto` | 200, 400, 404 |
| PATCH | `/api/proveedores/{id}/desactivar` | Admin | - | `ProveedorResponseDto` | 200, 404 |

Ejemplo `POST /api/proveedores`

```json
{
  "nombre": "Proveedor nuevo",
  "documentoFiscal": "PR-100",
  "telefono": "2222-9999",
  "email": "ventas@proveedor.test",
  "direccion": "Masaya",
  "activo": true
}
```

### Compras

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/compras` | Admin | - | `List<CompraResponseDto>` | 200 |
| GET | `/api/compras/{id}` | Admin | - | `CompraResponseDto` | 200, 404 |
| POST | `/api/compras` | Admin | `CompraCreateDto` | `CompraResponseDto` | 201, 400, 404 |

Ejemplo `POST /api/compras`

```json
{
  "proveedorId": 1,
  "usuarioId": 1,
  "numeroFactura": "FC-2026042401",
  "impuesto": 2.5,
  "observaciones": "Compra de reposicion",
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 5,
      "precioUnitario": 7.5
    }
  ]
}
```

Validaciones:

- proveedor activo;
- usuario activo;
- al menos un detalle;
- cantidad > 0;
- precio unitario >= 0.

### Ventas

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/ventas` | Admin, Operario | - | `List<VentaResponseDto>` | 200 |
| GET | `/api/ventas/{id}` | Admin, Operario | - | `VentaResponseDto` | 200, 404 |
| POST | `/api/ventas` | Admin, Operario | `VentaCreateDto` | `VentaResponseDto` | 201, 400, 404 |

Ejemplo `POST /api/ventas`

```json
{
  "clienteId": 1,
  "usuarioId": 2,
  "numeroComprobante": "VT-2026042401",
  "impuesto": 1.5,
  "descuento": 0,
  "observaciones": "Venta de mostrador",
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 2,
      "precioUnitario": 11.5
    }
  ]
}
```

Validaciones:

- cliente activo;
- usuario activo;
- producto activo;
- stock suficiente;
- no se permiten ventas sin detalles;
- total no puede ser negativo.

### Reportes

| Metodo | Ruta | Rol | Entrada | Salida | Codigos |
|---|---|---|---|---|---|
| GET | `/api/reportes/inventario` | Admin | - | `List<ReporteInventarioDto>` | 200 |
| GET | `/api/reportes/stock-bajo` | Admin | - | `List<StockBajoDto>` | 200 |
| GET | `/api/reportes/ventas-por-fecha` | Admin | Query fechas | `ReporteVentasPorFechaDto` | 200 |
| GET | `/api/reportes/compras-por-fecha` | Admin | Query fechas | `ReporteComprasPorFechaDto` | 200 |

Ejemplo `GET /api/reportes/ventas-por-fecha?fechaInicio=2026-04-01&fechaFin=2026-04-30`

```json
{
  "fechaInicio": "2026-04-01T00:00:00",
  "fechaFin": "2026-04-30T00:00:00",
  "totalPeriodo": 245.80,
  "movimientos": [],
  "totalesPorDia": [],
  "productosMasVendidos": []
}
```

## DTOs principales

- `LoginRequestDto`
- `LoginResponseDto`
- `UsuarioCreateDto`
- `UsuarioUpdateDto`
- `UsuarioResponseDto`
- `ProductoCreateDto`
- `ProductoUpdateDto`
- `ProductoResponseDto`
- `ClienteCreateDto`
- `ClienteUpdateDto`
- `ClienteResponseDto`
- `ProveedorCreateDto`
- `ProveedorUpdateDto`
- `ProveedorResponseDto`
- `CompraCreateDto`
- `CompraDetalleCreateDto`
- `CompraResponseDto`
- `VentaCreateDto`
- `VentaDetalleCreateDto`
- `VentaResponseDto`
- `ReporteInventarioDto`
- `StockBajoDto`

## Migraciones y base de datos

### DbContext

Archivo principal:

- `FerreteriaInventario.Api/Data/AppDbContext.cs`

### Comandos utiles

#### Package Manager Console

```powershell
Add-Migration InitialCreate -Project FerreteriaInventario.Api -StartupProject FerreteriaInventario.Api
Update-Database -Project FerreteriaInventario.Api -StartupProject FerreteriaInventario.Api
Remove-Migration -Project FerreteriaInventario.Api -StartupProject FerreteriaInventario.Api
```

#### dotnet CLI

```bash
dotnet restore
dotnet build
dotnet ef migrations add InitialCreate --project FerreteriaInventario.Api --startup-project FerreteriaInventario.Api
dotnet ef database update --project FerreteriaInventario.Api --startup-project FerreteriaInventario.Api
dotnet run --project FerreteriaInventario.Api
```

### Regenerar base en desarrollo

1. Eliminar el archivo `ferreteria_inventario.db`.
2. Ejecutar de nuevo la API.
3. El `DbInitializer` volvera a crear estructura y seed.

## Instalacion y ejecucion

### Requisitos

- Visual Studio 2022 con workload de `ASP.NET and web development`
- Visual Studio 2022 con workload de `.NET Multi-platform App UI development`
- SDK .NET 9

### Pasos

1. Clonar el repositorio.
2. Abrir `FerreteriaInventarioApp.sln`.
3. Restaurar paquetes NuGet.
4. Ejecutar primero `FerreteriaInventario.Api`.
5. Verificar Swagger en `http://localhost:5099/swagger`.
6. Configurar `ApiSettings.cs` si se usara Android Emulator o un dispositivo fisico.
7. Ejecutar `FerreteriaInventario.Maui` en Windows o Android.

## Comandos utiles

```bash
dotnet restore
dotnet build FerreteriaInventarioApp.sln
dotnet run --project FerreteriaInventario.Api
dotnet ef migrations add InitialCreate --project FerreteriaInventario.Api --startup-project FerreteriaInventario.Api
dotnet ef database update --project FerreteriaInventario.Api --startup-project FerreteriaInventario.Api
```

## Git y repositorio

Comandos sugeridos:

```bash
git init
git add .
git commit -m "Initial commit - FerreteriaInventarioApp"
```

## Capturas sugeridas

- Login
- Dashboard
- Productos
- Registro de venta
- Registro de compra
- Reportes

## Decisiones tecnicas

- **API separada**: facilita reutilizacion, pruebas y futura integracion con otros clientes.
- **SQLite**: ideal para desarrollo local simple y rapido.
- **EF Core**: acelera mapeo, relaciones y migraciones.
- **.NET MAUI**: permite una sola base de codigo multiplataforma.
- **MVVM**: separa logica de presentacion y vistas XAML.
- **JWT**: autenticacion moderna y desacoplada.
- **DTOs**: evitan exponer entidades persistentes.
- **Seed**: acelera validacion funcional y demostraciones.

## Limitaciones actuales

- No hay sincronizacion offline avanzada.
- No hay modulo de facturacion fiscal.
- No se implemento auditoria historica avanzada.
- Los reportes son basicos y no incluyen graficos.
- La gestion de clientes y proveedores en MAUI se centra en alta y consulta; el backend ya queda preparado para extender edicion/desactivacion.

## Mejoras futuras

- sincronizacion en la nube;
- lector de codigo de barras;
- exportacion PDF y Excel;
- auditoria por usuario;
- permisos mas granulares;
- dashboards con graficos;
- backups automaticos;
- integracion con facturacion electronica.

## Pruebas manuales recomendadas

1. Iniciar sesion con `admin`.
2. Verificar que aparezcan menus de compras, proveedores, usuarios y reportes.
3. Iniciar sesion con `operario`.
4. Verificar que no aparezcan menus administrativos.
5. Registrar una venta y validar disminucion de stock.
6. Registrar una compra y validar incremento de stock.
7. Intentar vender mas stock del disponible y comprobar error.
8. Consultar Swagger y probar endpoints protegidos con y sin token.
9. Verificar que al desactivar un usuario no pueda iniciar sesion.
10. Consultar el reporte de stock bajo.

## Criterios de aceptacion

- La solucion contiene dos proyectos: API y MAUI.
- La API expone endpoints REST documentados.
- Swagger se publica correctamente.
- SQLite se crea en desarrollo.
- Existe seed coherente con usuarios, catalogos, compras y ventas.
- El login funciona con Admin y Operario.
- Los endpoints protegidos aplican autenticacion y roles.
- La app MAUI inicia sesion y consume la API.
- La app MAUI lista productos.
- La app MAUI registra ventas.
- La compra aumenta stock.
- La venta reduce stock.
- No se permite vender por encima del stock disponible.
- El `README.md` permite a otro desarrollador poner en marcha el proyecto.
- El `.gitignore` evita subir basura tecnica o archivos sensibles.
