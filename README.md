# WEB_API_N10

Plantilla base modular, limpia y desacoplada para el desarrollo de APIs RESTful en **.NET 10**, con arquitectura N-Capas, **Entity Framework Core (Database First & Code First)**, **AutoMapper**, **Scalar UI** y autenticación **JWT Bearer** autónoma.

---

## 🚀 Arquitectura del Proyecto

- **`Controllers/`**: Puntos de entrada HTTP REST documentados con OpenAPI y Scalar UI.
- **`RN/` (Reglas de Negocio)**: Capa de servicios y lógica inyectada automáticamente mediante reflexión (`AddConfRN`).
- **`DTOs/`**: Objetos de transferencia fuertemente tipados para peticiones y respuestas.
- **`Context/` & `Models/`**: Entity Framework Core con soporte para SQL Server.
- **`Common/`**: Patrón de respuesta estandarizado `Result<T>` y extensión `.ToResult()`.
- **`Filters/`**: Filtro global de excepciones y validación por Api-Key.

---

## 🗄️ Guía de Base de Datos (Scaffold-DbContext / Database First)

Para generar automáticamente tus modelos y el `DbContext` a partir de una base de datos SQL Server existente:

### 1. En Package Manager Console (Visual Studio)

```powershell
Scaffold-DbContext "Name=ConnectionStrings:Default" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Context -OutputDir Models -Context MiAppDbContext -DataAnnotations -Force
```

#### Con tablas específicas:
```powershell
Scaffold-DbContext "Name=ConnectionStrings:Default" Microsoft.EntityFrameworkCore.SqlServer -ContextDir Context -OutputDir Models -Context MiAppDbContext -Tables Usuarios,Roles,Productos -UseDatabaseNames -NoPluralize -DataAnnotations -Force
```

### 2. En Terminal CLI (`dotnet ef`)

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Default" Microsoft.EntityFrameworkCore.SqlServer --context-dir Context --output-dir Models --context MiAppDbContext --data-annotations --force
```

### 3. Registro en `Program.cs`
Reemplaza el contexto de prueba por el generado:

```csharp
builder.Services.AddDbContext<MiAppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

---

## 🔐 Autenticación JWT

La plantilla incluye generación y validación de tokens JWT sin dependencias externas:

- **Login:** `POST /api/Auth/login`
- **Registro:** `POST /api/Auth/register`
- **Perfil Protegido:** `GET /api/Auth/me` (Requiere `Authorization: Bearer <TOKEN>`)

Configuración en `appsettings.json`:

```json
"Jwt": {
  "Key": "TuClaveSecretaPersonalDeAlMenos32Caracteres!",
  "Issuer": "WEB_API_N10",
  "Audience": "WEB_API_N10",
  "Expires": "1440"
}
```

---

## 🛠️ Ejecución Local

```bash
dotnet run --project API1/API1.csproj
```

Visita `https://localhost:7082/scalar/v1` para interactuar con la documentación en **Scalar UI**.
