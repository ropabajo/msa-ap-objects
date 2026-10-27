# Generación temporal de archivos mediante FileJobs

Objects conserva un único protocolo de generación: FileJobs. Registra intención,
estado, intentos y artefactos; el dominio productor consulta sus datos y genera el
contenido. Para miembros, Church genera el XLSX y lo sube con una autorización
temporal. Objects no recibe filas completas ni genera ese libro.

## Configuración

`FileJobs` define la activación, ambiente, audiencia técnica, clientes admitidos,
plazos, retención, reutilización y límites del registro durable. Permanece apagado
hasta completar la preparación DEV. `ConnectionStrings:FileJobs` identifica el
contexto EF separado sobre db_object de desarrollo en el destino confirmado 3306.
La aplicación no crea tablas ni ejecuta migraciones al arrancar.

Se reutilizan RabbitMq y Keycloak.Authority del MSA y la identidad MinIO restringida
al bucket church-dev. Los nombres de exchange y routing key pertenecen al productor;
las colas y bindings están en las definiciones existentes del broker de recetas.
No hay Broker anidado ni cuentas Rabbit por exportación.

## Protocolo público

La web usa los endpoints `/v1/church/{churchCode}/file-jobs` para solicitar, listar,
consultar, cancelar y obtener firmas de descarga por artefacto. La aceptación usa
HTTP 202 y el sobre Result; fallos esperados se representan con ProblemDetails.
El historial se limita al usuario, iglesia y ambiente autorizados. La firma es
renovable y temporal; no se guarda como enlace permanente.

El worker utiliza `/internal/v1/file-jobs` con su identidad técnica Keycloak y
lease vigente. RabbitMQ entrega referencias pequeñas, sin filas ni JWT. La outbox
publica con confirmación; el consumidor aplica ACK solo después de una decisión
durable. Reentregas y callbacks tardíos respetan los intentos y la idempotencia.

## Retiro del prototipo

Se eliminaron la sección TemporaryExports, las rutas
`/v1/church/{churchCode}/exports`, el coordinador basado en manifiestos MinIO,
su worker, el contrato de filas JSON y la dependencia ClosedXML. MemberExports
se retira en la rama de Church correspondiente; la web del piloto ya usa FileJobs.
La implementación anterior permanece en el historial Git.

Los endpoints genéricos siguen rechazando el prefijo temporary-exports/ para
conservar la protección de posibles artefactos anteriores. Retirar el código no
borra archivos, cambia permisos de almacenamiento ni ejecuta limpieza sobre DEV.

Activación, respaldos y comprobaciones pendientes:
[guía del piloto](https://github.com/church-ropabajo/docs/blob/codex/result-file-jobs/naza/backend/piloto-archivos.md).
Este cambio está preparado en la rama; no activa FileJobs ni modifica el runtime.

## Contrato de subida firmado

El permiso devuelve explícitamente `Fields["Content-Type"]` con el MIME reservado.
MinIO SDK 7 incluye esa condición en la política, pero omite el campo del formulario;
el worker necesita ambos para la subida multipart. La prueba de contrato ejercita
el generador de permisos y el cliente real del worker contra un receptor HTTP local
que rechaza el MIME ausente o distinto. No se añadieron opciones de configuración.

La comprobación nativa del candidato en DEV generó un XLSX sintético y verificó
POST firmado, tamaño/MIME/SHA256, GET firmado y recuperación del objeto de prueba.
El objeto y los archivos temporales se retiraron; no se consultó SQL ni se reinició
la API. Esto acredita almacenamiento y el transporte del candidato, no la
activación completa de FileJobs, Keycloak o el navegador.
