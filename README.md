# OrderFlow

Monorepo de la prueba técnica: Angular 17, AuthService .NET 8, Laravel/PHP 8 y SQL Server.

## Estructura

- `database/`: esquema y migración de `CustomerOrdersDB`.
- `auth-service/`: API .NET para registro, login, JWT y sesiones refresh.
- `customer-service/`: API Laravel para clientes, pedidos y dashboard.
- `frontend/`: aplicación Angular 17 con login/registro y consola de operaciones.

## Desarrollo local

Usa SQL Server `SQLEXPRESS06` y aplica la migración `database/09_auth_identity_migration.sql` solo si tu base existente aún no tiene `DisplayName` y `RefreshSessions`. No ejecutes las migraciones Laravel por defecto ni vuelvas a cargar el seed sobre una base con datos.

En terminales separadas, desde la raíz del repositorio:

```powershell
dotnet run --project auth-service/src/AuthService.Api/AuthService.Api.csproj
npm --prefix frontend start -- --host 127.0.0.1
```

En otra terminal inicia Laravel:

```powershell
Push-Location customer-service
php artisan serve --host 127.0.0.1 --port 8000
```

La API .NET usa `localhost:5087`, Laravel `localhost:8000` y Angular `localhost:4200`. La configuración local de AuthService usa Windows Authentication para SQL Server. Laravel toma su configuración de `customer-service/.env`; no compartas ni subas ese archivo.

## Stack Docker

Compose usa un SQL Server 2022 independiente, con el volumen `orderflow_sql_data`; no modifica la instancia local `SQLEXPRESS06` ni su base actual. Genera los secretos una vez y levanta los servicios con:

```powershell
.\scripts\New-LocalEnv.ps1
docker compose up --build
```

El stack publica Angular en `http://localhost:4200`, AuthService en `http://localhost:5087` y Laravel en `http://localhost:8000`.

## Validación

```powershell
dotnet test auth-service/AuthService.slnx --no-restore
npm --prefix frontend run build
$env:CHROME_BIN = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
npm --prefix frontend test -- --watch=false --browsers=ChromeHeadless
Remove-Item Env:CHROME_BIN
```

Las pruebas Laravel se ejecutan desde su carpeta:

```powershell
Push-Location customer-service
php artisan test --compact
Pop-Location
```

## Pendiente de entorno

XAMPP usa PHP 8.2 con los drivers oficiales Microsoft SQLSRV/PDO_SQLSRV 5.12 y ODBC 18. El flujo real .NET→JWT→Laravel→SQL Server fue probado y pasó; Microsoft certifica la línea 5.12 hasta SQL Server 2022, no para la instancia local SQL Server 2025. El contenedor Laravel usa PHP 8.3 y el driver 5.13, que sí contempla SQL Server 2025.

El servicio PHP usa Laravel 12.69, PHPUnit 11 y PHP 8.2. El lockfile quedó actualizado y `composer audit` no reporta advisories.

`docker compose config` pasa. La compilación de imágenes queda pendiente de red: Docker Desktop no pudo resolver Docker Hub/Microsoft al descargar las imágenes base. Cuando el daemon tenga acceso DNS a esos registries, ejecuta el comando de arriba para construir y levantar el stack.
