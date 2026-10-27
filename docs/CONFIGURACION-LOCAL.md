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
| `FileJobs` | Enabled=true en la revisión de activación DEV; registro durable, retención y límites del único flujo de generación |
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

La integración conserva el destino SQL Local 3306 y su dialecto verificado,
e incorpora los casos de archivos e importaciones descritos a continuación.


## Trabajos de archivos (piloto)

Se retiraron TemporaryExports, sus endpoints de filas JSON, coordinador, worker y
generador ClosedXML. El único protocolo de generación es FileJobs; Church genera
el XLSX por páginas y lo sube directamente con una autorización temporal. Las
rutas genéricas conservan bloqueado temporary-exports/ para no abrir el acceso a
posibles artefactos anteriores. Este cambio de fuente no borra archivos ni tablas.

Las opciones nuevas están versionadas en `appsettings.Local.json` y se incorporan
al contexto sanitizado de la imagen desde ese mismo archivo. La revisión de
activación habilita FileJobs en Local después de preparar las dependencias y el
respaldo recuperado. Producción no cambia. Ver el procedimiento
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

## RabbitMQ según el estándar del proyecto

La conexión se define una sola vez en RabbitMq y utiliza los Secrets existentes
del MSA. FileWorker/FileJobs no contienen Broker, Queue ni Exchange. Los nombres
se definen en los productores/consumidores de Infrastructure y en
k8s-recipes/rabbit-mq/definitions-configmap.yaml, como los demás mensajes.

El piloto reutiliza nc.objects.exchange, de tipo direct, dentro de local-vhost:

| Routing key | Cola |
| --- | --- |
| nc.objects.file-jobs.church.requested | nc.churches.file-jobs.queue |
| nc.objects.file-jobs.church.retry | nc.churches.file-jobs.retry.queue |
| nc.objects.file-jobs.church.failed | nc.churches.file-jobs.dead.queue |

La cola específica evita competir con los eventos de personas de nc.churches.queue.
No se añaden exchanges ni cuentas Rabbit por feature. Los permisos se verifican
sobre la cuenta DEV que ya utiliza el microservicio. La preparación posterior
instaló las tres colas/bindings en local-vhost con recuperación previa verificada.
La revisión de activación pone FileJobs.Enabled=true en el fuente y en la imagen
sanitizada. Ambos Secrets están preparados y el token técnico DEV fue verificado.
El pipeline de verificación reutiliza el workflow Local en modo publish_only:
publica un digest tras las pruebas y omite el job SSH. La activación coordinada
usa el operador YAML común desde codex-dev; no requiere copiar su clave administrativa.
dev-file-jobs-objects conserva solo la conexión 3306 aprobada. Estado operativo:
[FileJobs DEV](https://github.com/ropabajo/k8s-recipes/blob/codex/file-generation-standard/environments/church-dev/FILE-JOBS.md).

El código fija ap-framework 87e1598: RabbitMq 1.1.0-dev.2, Files 1.0.0-dev.4 y
Authz 1.1.4-dev.2. AddRabbitMq descubre los productores de IProducer<T> y comparte
su fábrica. FileJobProducer define exchange/routing key y ChurchFileJobConsumer
define QueueName y ruta de retry. Se conservan confirms, persistencia y ACK manual.

Worker deriva TokenEndpoint de Keycloak.Authority, StorageHosts de MinIo.Endpoint
y utiliza RabbitMq.MaxRetryAttempts (tres por defecto) para reintentos y su Producer se fija al registrar
AddFileWorker<ChurchFileJobConsumer>(configuration, "church"). Objects valida el
JWT técnico con esa misma Authority. Se mantienen ClientId/ClientSecret del worker
y su audiencia, que autorizan HTTP y no constituyen otra conexión Rabbit.

Las imágenes se derivan del fuente; no se recrean Broker anidados durante el
saneamiento. Los workflows de verificación prueban las clases reales contra las
definiciones canónicas en RabbitMQ/MySQL efímeros. Publican candidatos con los
flags del fuente y omiten el despliegue SSH en publish_only.

El 26/10/2026 se desplegó el candidato de fuente 372b763 como codex-dev, con
FileJobs activo. La configuración sanitizada incluida en el Pod coincide con la
derivada del fuente. El backend completó un XLSX sintético de ocho columnas sin
filas personales mediante RabbitMQ, worker con JWT técnico y MinIO; se verificaron
descarga y aislamiento de propietario/iglesia en el servicio y se retiró la prueba.
La API de usuario rechazó el token técnico y la petición anónima (401). El login
y flujo completo de una persona en navegador continúan pendientes. Digests,
respaldos y evidencias están en la guía operativa FileJobs DEV enlazada arriba.

Program.cs conserva 26 líneas; ApiServiceRegistration compone filtros y JWT técnico,
y Infrastructure registra FileJobs. El Secret dev-file-jobs-objects conserva solo
ConnectionStrings__FileJobs; dev-file-jobs-church conserva FileWorker__ClientSecret.
Las credenciales Rabbit se siguen obteniendo del runtime actual de cada MSA.

El contrato Local de importación es v2/churches/{churchCode}/imports. Exige
ActiveChurchRoute e IamAdmin y conserva la huella del solicitante. La ruta manda
sobre el body; se comprueba también la propiedad del objeto subido. Listado,
resultados y recuperación consultan solo la iglesia y el usuario autenticados.
El controller legacy sin ámbito se retira en Local y permanece fuera de Local.
No hay nuevas claves de conexión: registro y outbox usan DefaultConnection.
Aplicar primero la migración incremental db_object y la plantilla única nueva.

## Columnas de integración normalizadas en DEV

Integration 1.0.0-dev.2 usa columnas físicas españolas y claves numéricas internas.
La revisión fijada de framework es 6ab2e834262a68c17930bd3b1b2360f7f15e5ee9.
Los códigos públicos, valores y contratos HTTP/RabbitMQ se conservan. Antes de
arrancar este modelo sobre un esquema anterior, aplicar la migración incremental
20261027-integration-columnas-espanol, con respaldos y escritores detenidos; no
ejecutar init.sql ni DDL al arrancar. La migración ya está aplicada en church-dev.

La adopción es explícita por servicio: no crea tablas en otras bases por instalar
la librería. Procedimiento, comprobaciones y reversión:
[operador común](https://github.com/ropabajo/k8s-recipes/blob/development/environments/church-dev/MIGRACION-COLUMNAS-IMPORTACION.md).
