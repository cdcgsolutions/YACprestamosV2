# Guía de Uso de la Plantilla API1

Esta guía describe cómo instalar, verificar y utilizar la plantilla de proyecto **API1**.

## Requisitos Previos

- .NET 10 SDK (o la versión compatible con `net10.0`).
- Acceso al feed de NuGet de Azure DevOps (si se requiere para paquetes privados).

## Instalación de la Plantilla

Para instalar la plantilla localmente desde el código fuente:

1.  Abra una terminal en el directorio raíz del proyecto de la plantilla (donde se encuentra la carpeta `.template.config`).
    ```powershell
    cd C:\Users\Manfredo.Agreda\source\repos\GP\Projects\Templates\APINetCore\API1
    ```

2.  Ejecute el comando de instalación de `dotnet new`:
    ```powershell
    dotnet new install .
    ```

   Esto instalará la plantilla con el nombre corto `netcoreapi`.

## Verificación de la Instalación

Para verificar que la plantilla se ha instalado correctamente, ejecute:

```powershell
dotnet new list netcoreapi
```

Debería ver la plantilla listada con el nombre "Plantilla NetCore API Modular".

## Creación de un Nuevo Proyecto

Para crear un nuevo proyecto basado en esta plantilla, utilice el siguiente comando:

```powershell
dotnet new netcoreapi -n MiNuevoProyectoAPI -o .\MiNuevoProyectoAPI
```

- `-n`: Nombre del proyecto (y de la solución).
- `-o`: Directorio de salida.

### Personalización

La plantilla generará un nuevo proyecto donde:
- El nombre del proyecto será `MiNuevoProyectoAPI`.
- El espacio de nombres raíz será `MiNuevoProyectoAPI`.
- Se generarán puertos aleatorios para `http` y `https` en `launchSettings.json` para evitar conflictos.

## Estructura del Proyecto Generado

El proyecto generado tendrá la siguiente estructura base:

- **Configuration/**: Configuraciones de la aplicación.
- **Context/**: Contexto de base de datos (Entity Framework).
- **Controllers/**: Controladores de la API.
- **DTOs/**: Objetos de transferencia de datos.
- **Filters/**: Filtros de acción/excepción.
- **Helpers/**: Clases de ayuda y utilidades.
- **Middleware/**: Middleware personalizado.
- **Models/**: Modelos de dominio.
- **RN/**: Reglas de Negocio (Business Rules).

## Desinstalación de la Plantilla

Si desea desinstalar la plantilla:

```powershell
dotnet new uninstall .
```
(Ejecutando desde el directorio de la plantilla).

O mediante el nombre del paquete si fue instalada desde un NuGet:

```powershell
dotnet new uninstall Template.API1
```
