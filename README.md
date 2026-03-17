# TaskCalendar

Aplicacion de calendario multiusuario construida con `.NET 9`, `ASP.NET Core`, `Entity Framework Core`, `MAUI Blazor Hybrid` y `xUnit`.

## Alcance de esta primera entrega

- Login y registro con `JWT`
- Usuario demo con seed inicial
- Calendario diario, semanal y mensual
- Tareas con prioridad, estado, descripcion y repeticion
- Horario operativo por usuario y por dia
- Regla de solapamiento: se permiten hasta 5 minutos
- `Connection String` configurable
- Tests unitarios para recurrencia y validacion

## Estructura

- `src/TaskCalendar.Api`: API ASP.NET Core con auth, seed y endpoints de agenda
- `src/TaskCalendar.App`: cliente `MAUI Blazor Hybrid` orientado a Windows
- `src/TaskCalendar.Domain`: entidades y enums
- `src/TaskCalendar.Application`: DTOs y reglas de negocio reutilizables
- `src/TaskCalendar.Infrastructure`: EF Core, Identity, JWT y configuracion
- `tests/TaskCalendar.Tests`: tests unitarios

## Credenciales demo

- Usuario: `demo@taskcalendar.local`
- Password: `Demo123!`

## Connection String

La configuracion por defecto esta en [appsettings.json](/C:/Users/cvirg/OneDrive/Documents/New%20project/src/TaskCalendar.Api/appsettings.json) y usa `SQL Server LocalDB` con autenticacion de Windows.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TaskCalendarDevDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
},
"Database": {
  "Provider": "SqlServer"
}
```

Mas adelante solo vas a tener que reemplazar esa cadena por la de produccion.

Ejemplo de configuracion SQL Server:

```json
"Database": {
  "Provider": "SqlServer"
},
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TaskCalendarDevDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

## Como ejecutar

1. Restaurar paquetes:

```powershell
dotnet restore TaskCalendar.sln --configfile NuGet.Config
```

2. Levantar la API:

```powershell
dotnet run --project .\src\TaskCalendar.Api
```

3. En otra terminal, ejecutar la app Windows:

```powershell
dotnet build .\src\TaskCalendar.App\TaskCalendar.App.csproj --configfile NuGet.Config
dotnet run --project .\src\TaskCalendar.App\TaskCalendar.App.csproj -f net9.0-windows10.0.19041.0
```

4. Ejecutar tests:

```powershell
dotnet test .\tests\TaskCalendar.Tests\TaskCalendar.Tests.csproj --configfile NuGet.Config
```

## Notas

- La app cliente esta enfocada en Windows para esta iteracion.
- La URL de la API en el cliente se configura en [MauiProgram.cs](/C:/Users/cvirg/OneDrive/Documents/New%20project/src/TaskCalendar.App/MauiProgram.cs).
- El seed crea un usuario demo, horario operativo base y tareas recurrentes de ejemplo.
