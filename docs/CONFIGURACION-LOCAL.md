# Configuración Local y church-dev

La configuración pública está en [src/Ropabajo.Church.Sanluis.Objects.Api/appsettings.Local.json](../src/Ropabajo.Church.Sanluis.Objects.Api/appsettings.Local.json). El workflow
`.github/workflows/development.yml` usa el ambiente .NET **Local** en church-dev.
`appsettings.Development.json` pertenece al despliegue existente de producción y
no se utiliza como sustituto de Local.

| Sección | Valor o procedencia |
| --- | --- |
| `ConnectionStrings.DefaultConnection` | Plantilla pública de db_object; credenciales mediante Secret propio |
| `Redis` | `redis-shared.church-dev.svc.cluster.local:6379`, Database `1`, Ssl `false`; User público `msa-iam`, Password vacío |
| `RabbitMq` | `rabbitmq-shared.church-dev.svc.cluster.local:5672`, VirtualHost `local-vhost`, UseSsl `false`; usuario y contraseña mediante Secret compartido |
| `Keycloak` | Authority `https://auth-dev.idnsanluis.org/realms/church`, Audience `account`, AuthorizedParty/RoleClient `web-ap-container`, Enabled y RequireHttpsMetadata `true` |
| `MinIo` | `minio-api.ropabajo.pe`, UseSsl `true`, BucketName `church-dev`; claves vacías en el fuente |
| `Base.Path` | Cadena vacía: Nginx retira el prefijo público al enviar a la API |
| `spring.cloud.config.enabled` | `false`: configuración Local sin servidor Spring Config |
| `Logging` / `AllowedHosts` | Default `Warning`, Microsoft.Hosting.Lifetime `Information`; AllowedHosts `*`, como los defaults previos de la imagen |

## SQL y secretos

El fuente conserva esta plantilla sin credenciales:

```text
server=mysql-local.church-dev.svc.cluster.local;database=db_object;user=;password=;port=3306;SslMode=None;
```

El valor completo de `ConnectionStrings__DefaultConnection` se suministra mediante
`dev-objects-runtime`. La plantilla de Git no permite autenticarse y no reemplaza el
archivo privado que se empleó para la carga inicial del runtime. Para ejecutar la
API desde el equipo del desarrollador, suministrar una cadena privada alcanzable
desde ese equipo mediante variables de entorno o el mecanismo local de secretos.
Los nombres `*.svc.cluster.local` se resuelven dentro de church-dev.

Redis User/Password se inyectan desde `dev-shared-runtime`. Los seis campos de
Redis están visibles en el setting: Host, Port, User, Password, Database y Ssl.
La imagen vacía también User; Kubernetes aporta el usuario real del Secret.
Database 1 separa claves de DB0, pero no concede permisos ACL ni aísla recursos
físicos. Los permisos de los prefijos deben estar autorizados en Redis.

MinIo usa Endpoint `minio-api.ropabajo.pe`, TLS y bucket privado `church-dev`. AccessKey/SecretKey provienen de `dev-objects-runtime` y corresponden a la identidad restringida de Objects. SQL Local utiliza db_object en `161.132.54.155:3306`, con el alias `mysql-local.church-dev.svc.cluster.local:3306` y la cuenta DEV `usr_object` suministrada por el gestor privado.

El runtime histórico usaba `mysql-objects-shared`, cuyo EndpointSlice traducía al
3307 compartido con producción. Se conserva como antecedente y respaldo privado;
no es el destino de este corte. Cambiar el fuente no actualiza el Secret activo:
la transición sustituye únicamente `ConnectionStrings__DefaultConnection`, con
precondiciones de UID/resourceVersion y conservando las claves MinIO existentes.
No crea base, tablas, usuarios ni permisos y no ejecuta el init.sql.

Los Secrets compartidos pueden contener claves para otros consumidores. Solo se
documentan aquí las integraciones utilizadas por este aplicativo; no se agregan
secciones vacías para fingir una dependencia que no consume.

## Precedencia y publicación

Con la configuración estándar del host .NET, el orden relevante es
`appsettings.json` → `appsettings.Local.json` → variables de entorno. Por ejemplo,
`Redis__Database` sobrescribe `Redis.Database`; el Secret de SQL sobrescribe la
cadena completa. Un cambio en Git no actualiza un Secret ya instalado.

La compilación toma los valores públicos del setting fuente y elimina las
credenciales de los settings publicados. El contenedor incluye únicamente los
settings base mínimos y Local sanitizados. No incluir archivos privados del
operador ni copiar valores de Kubernetes a Git o a la imagen.

La receta define variables propias del proceso, que no son secciones de settings:

- `ASPNETCORE_ENVIRONMENT=Local` selecciona el archivo del ambiente.
- `ASPNETCORE_URLS=http://+:8080` configura la escucha interna.
- `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` desactiva recarga de archivos.

Las revisiones de recetas y la imagen por digest siguen fijadas por el workflow.
El despliegue conserva los ConfigMap y Secrets existentes. Esta corrección alinea
los defaults del fuente y de CI con el contrato Local; no regenera credenciales.

## Validación de correspondencia

Con los checkouts hermanos y `jq`, `yq` (Mike Farah), Git y Python 3 disponibles:

```bash
bash ../k8s-recipes/environments/church-dev/operations/audit-local-settings.sh .. objects
```

El auditor compara fuente, Deployment de la revisión fijada en CI y settings
sanitizados por el generador real del pipeline. Comprueba esquema Redis, tipos,
plantilla SQL y ausencia de credenciales. No se conecta al servidor ni lee Secrets.
El informe general está en
[k8s-recipes: auditoría Local](https://github.com/ropabajo/k8s-recipes/blob/development/environments/church-dev/AUDITORIA-SETTINGS-LOCAL.md).

Antes del corte se verificaron desde church-dev MySQL 5.7.44, las doce tablas y
sus claves esperadas mediante una transacción de solo lectura. El respaldo DEV
se restauró en un Pod aislado con MySQL 5.7.44 fijado por digest: coinciden
esquema, índices y checksums de datos; la base fuente se conserva. También se
ensayó restaurar el Secret en un nombre temporal y se retiraron los recursos de
prueba. Los archivos privados permanecen en la VM, con permisos 700/600.

Auditoría del 25/10/2026 (America/Lima): los ocho MSA pasaron el auditor común
contra sus recetas fijadas y los settings sanitizados por sus workflows reales.
Objects conserva el pin b8f02b2; este ajuste no cambia CI ni settings productivos.
Las 54 columnas mapeadas por sus seis entidades legacy existen en db_object DEV.

El registro de persistencia usa el dialecto MySQL 5.7.44 en Local, que coincide
con el servidor DEV comprobado, y conserva 8.0.21 para los demás ambientes.
La configuración y el ambiente se reciben directamente desde el host.

Este cambio ajusta el destino y el dialecto SQL Local y su documentación. La rama
del piloto de archivos permanece separada; no incorpora sus casos de uso,
workers, opciones o nuevos endpoints al despliegue base.
