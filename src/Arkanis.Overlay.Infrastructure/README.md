# `Arkanis.Overlay.Infrastructure`

## Database

### Migrations

#### Adding a migration

```shell
dotnet ef migrations add \
    --project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    --startup-project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    --context Arkanis.Overlay.Infrastructure.Data.OverlayDbContext \
    --configuration Debug \
    --verbose Initial \
    --output-dir Data/Migration
```

#### Adding a migration against a specific database

```shell
dotnet ef migrations add \
    --project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    --startup-project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    --context Arkanis.Overlay.Infrastructure.Data.OverlayDbContext \
    --configuration Debug \
    --verbose Initial \
    --output-dir Data/Migration
    "Data Source=<PATH_TO_DATABASE_FILE>"
```

### Upgrading/Downgrading the database

```shell
dotnet ef migrations add \
    --project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    "Data Source=<PATH_TO_DATABASE_FILE>"
```

```shell
dotnet ef migrations add \
    --project src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj \
    "Data Source=<PATH_TO_DATABASE_FILE>"
    "<MIGRATION_NAME>"
```
