# Plantilla API .NET Core 10 (Arquitectura Modular & Independiente)

Esta plantilla proporciona una estructura base limpia, desacoplada y lista para producción utilizando **.NET Core 10**, **Entity Framework Core**, **Scalar UI**, **AutoMapper** y autenticación **JWT estándar**.

## 🚀 Características

- **Arquitectura N-Capas**:
  - `Controllers/`: Endpoints REST con OpenAPI y Scalar UI.
  - `RN/` (Reglas de Negocio): Capa de servicios inyectada automáticamente (`AddConfRN`).
  - `DTOs/`: Objetos de transferencia fuertemente tipados.
  - `Context/` & `Models/`: Entity Framework Core con SQL Server nativo.
  - `Common/`: Patrón `Result<T>` y métodos de extensión `.ToResult()`.
- **Autenticación JWT Nativa**: Generación y validación autónoma de tokens sin dependencia de servicios externos.
- **Documentación Moderna**: Interfaz interactiva de **Scalar UI** en `/scalar/v1`.
- **Observabilidad**: Health Checks en `/health` y soporte opcional para Application Insights.
- **100% Pública y Offline**: Sin dependencias de feeds privados ni redes corporativas.

## 🛠️ Configuración Rápida

### 1. Base de Datos
En `appsettings.json`, configura la cadena de conexión:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=MiProyectoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
}
```

### 2. Autenticación JWT
Personaliza la clave y opciones en `appsettings.json`:

```json
"Jwt": {
  "Key": "TuClaveSecretaPersonalDeAlMenos32Caracteres!",
  "Issuer": "MiApp",
  "Audience": "MiCliente",
  "ExpiresInMinutes": "120"
}
```

### 3. Ejecución

```bash
dotnet run
```

Visita `https://localhost:7082/scalar/v1` para interactuar con la documentación y probar los endpoints.
