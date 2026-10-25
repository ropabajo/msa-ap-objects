# Piloto de archivos: activación y comprobaciones

Fecha: 25/10/2026, America/Lima. Implementado en ramas de revisión; no desplegado.
La guía canónica es `church-ropabajo/docs/naza/backend/lineamientos-refactor.md`.

## Entrega de código

- ap-framework: Primitives 1.0.0-dev.1, Mediator/Core 1.1.0-dev.1, Files 1.0.0-dev.1.
  Consumidores fijan el commit fuente y construyen los paquetes en CI; no dependen
  de una versión dev supuestamente publicada en NuGet.
- Objects: registro SQL separado, outbox, dispatcher, intentos, firma y retención.
- Church: `GenerateMemberWorkbook`, lectura de páginas de 500 bajo REPEATABLE READ,
  Open XML y subida directa mediante la política temporal de Objects.
- ng-utils: `@ropabajo/shared/files` 0.7.17-dev.1, Signal Store e historial reutilizable.
- web-church: botón Excel con filtros aplicados y componente Mis archivos.
- k8s-recipes: referencias opcionales a Secrets de credenciales específicos.

El Excel nuevo no pasa por Church/Objects para descargarlo: el navegador recibe una
firma temporal de Objects y descarga de MinIO. En RabbitMQ viajan solo referencias.
El contenido de miembros nunca se transmite como JSON completo entre los servicios
nuevos. El prototipo previo se conserva durante el corte para no romper consumidores;
no es el patrón que deben copiar los módulos futuros.

## Preparación de DEV, antes de habilitar flags

1. Revisar y ejecutar una vez `msa-ap-objects/operations/file-jobs/001_metadata_dev.sql`
   con una identidad de migraciones en la instancia MySQL compartida de Objects.
   Crea únicamente la base lógica `db_file_jobs_dev`; **no ejecutar en db_object**.
   La API no ejecuta DDL. Crear un usuario de aplicación con SELECT/INSERT/UPDATE/DELETE
   solo en esa base y restringir su origen a la red verificada del servicio.
   Producción utilizará otra base y otra identidad; este script no migra producción.
2. Revisar `rabbitmq-development.json`: añade dos exchanges y tres colas exclusivos
   de DEV, sin usuarios ni contraseñas. Crear solo esos recursos mediante la interfaz
   administrativa del broker; no sustituir sus definiciones completas existentes.
   Usa `local-vhost`, cola principal durable classic, retry con TTL 30 s y DLQ.
   El broker actual de un nodo no ofrece replicación. Hacer respaldo antes del cambio
   operativo y ensayar recuperación; la outbox no es un respaldo del broker.
3. Crear identidades Rabbit separadas: `file-jobs-objects-dev` publica únicamente en
   `development.file-jobs.v1`, routing key `church`; `file-jobs-church-dev` consume
   `development.file-jobs.church.v1` y publica en el mismo exchange con routing key
   `retry.church`. Ninguno declara recursos; configure regex `^$`. Acotar read/write
   a esos nombres y usar topic permissions para las routing keys. No reutilizar las
   credenciales administrativas ni ampliar los permisos del usuario compartido.
4. Crear el cliente Keycloak `msa-church-file-worker` en el realm DEV `church`:
   confidencial, service accounts activas, client credentials; standard flow y
   direct access grants desactivados, sin roles administrativos. Audience mapper
   añade `objects-files` solo al access token. Vida del token: 300 s.
   Objects valida firma, issuer, audience, lifetime y `azp` permitido. No usar el
   JWT de una persona para el worker. Las rutas públicas siguen verificando IAM.
5. Confirmar que la identidad MinIO de Objects puede crear/verificar/retirar objetos
   de `church-dev/file-jobs/v1/development/` y no del bucket productivo. Church no
   recibe AccessKey/SecretKey de MinIO: recibe una autorización POST de objeto,
   tamaño y MIME exactos con vigencia limitada.
6. Crear por el gestor privado, sin imprimir valores, los Secrets siguientes:

| Secret | Claves |
| --- | --- |
| `dev-file-jobs-objects` | `ConnectionStrings__FileJobs`, `FileJobs__Broker__UserName`, `FileJobs__Broker__Password` |
| `dev-file-jobs-church` | `FileWorker__ClientSecret`, `FileWorker__Broker__UserName`, `FileWorker__Broker__Password` |

La conexión de metadatos usa el Service interno de MySQL Objects, base
`db_file_jobs_dev` y usuario propio. Las plantillas no contienen credenciales reales.
No añadir flags públicos dentro de los Secrets para ocultar diferencias con el fuente.

## Corte coordinado

- En una revisión de activación, cambiar `FileJobs.Enabled` (Objects) y
  `FileWorker.Enabled` (Church) a true en sus `appsettings.Local.json`; se copian
  de forma sanitizada a la imagen. Conservar los mismos valores públicos en Git.
- Desplegar Objects y comprobar registro, esquema, outbox y autorización técnica.
  Después Church y, finalmente, web-church con el shared source fijado por SHA.
- Probar desde sesión autorizada y otra iglesia: aceptar 202, preparar XLSX, descargar
  desde MinIO, renovar firma sin generar de nuevo, cancelación y cambio de ámbito.
- Confirmar consumidores del prototipo anterior antes de desactivar
  `TemporaryExports.Enabled` y `MemberExports.Enabled`; retirarlo en otra revisión.
  No eliminar rutas existentes durante una activación parcial.
- No promocionar automáticamente a producción. La validación de desarrollo y el plan
  productivo son pasos separados. No se ha ejecutado ninguno desde esta entrega.

## Operación y vigencias

- Un RequestId reintentado devuelve el mismo trabajo; reutilizarlo con otros filtros
  devuelve 409. Una intención nueva comparte un trabajo equivalente activo.
- Si está ready, los datos se reutilizan 300 s desde DataAsOf; Actualizar Excel puede
  crear una versión actual. Nunca se duplica una preparación equivalente en curso.
- Archivo Excel: 24 h desde generación. URL: hasta 15 min, limitada por el tiempo
  restante del archivo. Una descarga no amplía ninguna vigencia.
- Máximo de tres trabajos activos por usuario; 10 000 filas y 20 MiB por Excel;
  lectura de páginas de 500, un trabajo por consumidor.
- Lease: 120 s; heartbeat: 30 s; intento: 900 s; deadline de trabajo: 3600 s.
  Hasta tres intentos de generación, retry de 30/120 s con jitter.
- Falta de transporte: cola diferida 30 s, máximo 60 entregas antes de DLQ.
  Publicación no confirmada no se marca como exitosa. Revisar DLQ y jobId antes
  de redespachar: un job terminal no vuelve a generar al recibir un mensaje viejo.
- Cancelar solicita parada durable; cerrar la página no cancela el trabajo.
  Un callback con intento/lease vencido no puede publicar.
- Temporales locales abandonados: barrido después de 24 h. Archivos huérfanos y
  vencidos: retiro del prefijo reservado. Metadatos terminales: 30 días, después
  de confirmar retirada de todos sus artefactos. No aplica a fotografías ni PDF.
- PDF electorales: conservan el mecanismo histórico actual. Su migración al catálogo
  común es la siguiente fase; nunca aplicarles la expiración de Excel.

## Verificación realizada y límites

En cloud se probaron Result y su compatibilidad HTTP real (incluido 202), validación
Open XML con 10 000 filas, worker/heartbeat/respuesta perdida, confidencialidad HTTP,
MySQL 5.7 con concurrencia/fencing/retención y Church con 1203 miembros en tres páginas.
También se probaron publisher confirms, NO_ROUTE, retry diferido y DLQ con RabbitMQ
3.13 real, además del frontend en Chrome Headless. Ver informe del commit entregado.

Pendiente de activación: credenciales y esquema DEV, issuer/audience reales, subida
POST y descarga contra el MinIO del ambiente, reinicio/pérdida del broker, prueba
completa en navegador y medición de recursos bajo el Deployment real. No se pudo
obtener una imagen pública de MinIO en cloud; el test HTTP no sustituye esa prueba.
No se han verificado ni modificado servidores productivos, laptop o desktop.
