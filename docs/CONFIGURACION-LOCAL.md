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
| `TemporaryExports` | Enabled=true, RetentionHours=24, ReuseSeconds=300, LinkSeconds=900, InputTimeoutSeconds=120, PollSeconds=5, BatchSize=25, MaxAttempts=3, MaxRows=10000, MaxColumns=40 y MaxSourceBytes=10485760 |
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

MinIo usa Endpoint `minio-api.ropabajo.pe`, TLS y bucket privado `church-dev`. AccessKey/SecretKey provienen de `dev-objects-runtime` y corresponden a la identidad restringida de Objects. El destino SQL previsto es db_object de desarrollo en 161.132.54.155:3306. El runtime histórico en 3307 no se modifica por cambiar este archivo: comprobar y preparar el Secret antes de desplegar.

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


## Trabajos de archivos (piloto)

Las opciones nuevas están versionadas en `appsettings.Local.json` y se incorporan
al contexto sanitizado de la imagen desde ese mismo archivo. Permanecen desactivadas
hasta preparar la infraestructura. Producción no cambia. Ver el procedimiento
canónico en [docs/naza/backend/piloto-archivos.md](https://github.com/church-ropabajo/docs/blob/main/naza/backend/piloto-archivos.md).

Objects consume `FileJobs` y `ConnectionStrings:FileJobs`; Church consume `FileWorker`.
Las credenciales se inyectan por los Secrets específicos opcionales; no incluyen flags
públicos. El runtime usa la precedencia habitual: variables de entorno sobre JSON.
El usuario técnico Rabbit es distinto por servicio. Solo Objects conserva credenciales
MinIO, y el worker usa una política de subida temporal, sin claves permanentes.
En desarrollo el registro exige `db_object` en el mismo servidor/puerto que
`DefaultConnection`, mediante `mysql-local:3306` o `161.132.54.155:3306`.
La cuenta confirmada es `usr_object`; el fuente mantiene usuario y contraseña
vacíos. Los valores efectivos se inyectan mediante `dev-objects-runtime` y
`dev-file-jobs-objects`. Si el primero aún apunta a 3307, preparar la transición
antes de desplegar. El arranque Local rechaza el destino histórico incluso con
FileJobs apagado y utiliza el dialecto MySQL 5.7.44. Registrar dependencias no
abre SQL ni ejecuta migraciones.

El CI obtiene el `init.sql` canónico de docker-kubernetes-configs en una revisión
inmutable y proporciona su ruta con `OBJECTS_SCHEMA_INIT_SQL`. Las pruebas usan
solo el bloque de tareas dentro de MySQL efímero local. No hay otra copia SQL
en el MSA ni cambios a cuentas desde estas pruebas.

## Patrón RabbitMQ compartido

La rama del piloto fija ap-framework 264a9e5: RabbitMq 1.1.0-dev.1,
Files 1.0.0-dev.2 y Authz 1.1.4-dev.2. Files adapta el protocolo existente al
transporte común de RabbitMq, con confirms, persistencia, mandatory, prefetch uno,
ACK manual y retry diferido. Se conservan los perfiles de conexión y credenciales
separados; no se reemplaza la identidad del bus legacy ni cambia el JSON público.

El patrón para desarrollos nuevos está en los lineamientos canónicos de Naza y
[ap-framework/docs/RABBITMQ.md](https://github.com/robertpablo/ap-framework/blob/264a9e5b2814b53bf8e0cbf4460918a2f8232d35/docs/RABBITMQ.md).
La auditoría común de los ocho MSA pasó con Church/Objects fijados a recetas
9fae3db. Los workflows de verificación de estas ramas compilan y prueban contra
MySQL efímero; no publican imágenes ni despliegan. Esta adopción no habilita flags.

Objects declara FileJobProducer en Infrastructure, lo registra con
AddRabbitMqProducer y el dispatcher depende de IProducer<GenerateFileCommand>.
AddObjectsApi registra filtros y llama al JWT técnico común de Authz;
AddInfrastructure incorpora AddFileJobs. Program.cs conserva 26 líneas de
composición y middleware. El arranque desactivado se prueba sin red y verifica
un único productor y un único dispatcher.
