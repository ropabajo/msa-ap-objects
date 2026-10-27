# Contrato HTTP de la carga combinada

CombinedImportController enlaza HTTP, mapea RequestDto → Command/Query y despacha
mediante IMediatorBus. El propietario, la autorización de la carga y las reglas de
Persona/Miembro permanecen en Application. No accede a SQL, MinIO o RabbitMQ.

Las rutas mantienen v2/churches/{churchCode}/imports. ActiveChurchRoute comprueba
la iglesia activa y IamAdmin exige el permiso administrativo vigente. El actor no
se recibe desde el cliente. ChurchCode, LoadCode y RowCode del DTO se sustituyen
por los identificadores de ruta antes del mapping, conservando los contratos JSON.

| Método y ruta relativa | Éxito | Respuesta en value |
| --- | --- | --- |
| POST /imports | 202 | CreateCombinedImportResponseDto: loadCode y status |
| GET /imports | 200 | ListCombinedImportsResponseDto: items y total |
| GET /imports/{loadCode}/rows | 200 | ListCombinedImportRowsResponseDto: items y total |
| POST /imports/{loadCode}/rows/{rowCode}/retry | 202 | RetryCombinedImportRowResponseDto: accepted |

El prefijo completo incluye v2/churches/{churchCode}. Los éxitos conservan
isSuccess/value/errors de Result. Una página vacía es un éxito con items vacío y
total cero. Un 202 acepta la solicitud; no afirma que Persona o Miembro ya estén
importados. RequestId se conserva al repetir la misma intención HTTP. La cancelación
del request se propaga a MediatR; no cancela por sí sola un trabajo ya aceptado.

## Registro y consultas

Ejemplo de cuerpo para POST de registro, después de subir y confirmar el objeto:

```json
{
  "requestId": "8431e4c1-4a16-46ab-8932-e02851c073dd",
  "objectCode": "38d423f7-886b-4eaf-a757-37fe5bde5ac0",
  "formatCode": "82d1d1db-0727-421e-a0fe-78750146f433",
  "description": "Carga de personas y miembros"
}
```

El controller recibe metadatos JSON con el límite existente de 64 KiB; el archivo
se sube por separado. Reutilizar una intención con otros parámetros o un archivo
ya asociado produce 409. Un objeto no disponible para el usuario produce 404.

GET de cargas admite filterFormatCode, pageNumber y pageSize. GET de filas admite
stateCode, pageNumber y pageSize; estado vacío conserva la semántica Todos. La
paginación mantiene sus límites actuales (página desde 1 y tamaño entre 1 y 100).
Una carga ajena se consulta como no encontrada, sin revelar que existe.

El POST de recuperación recibe requestId. Application decide el paso recuperable
sin repetir Persona si ya está confirmada. Una fila o carga ajena/inexistente
produce 404; un estado no recuperable produce 409.

## Errores y filtro

La documentación XML y ApiExplorer declaran el sobre de éxito y los códigos HTTP
reales. La validación y los fallos esperados se representan con ProblemDetails;
los errores de Application conservan código, mensaje seguro y campo cuando aplica.
401 puede ser el challenge sin cuerpo. 403 y 503 permiten un objeto en OpenAPI:
el middleware Authz puede responder code/title, mientras MVC puede emitir
ProblemDetails. No se cambia ese contrato compartido para documentar el controller.

FileJobExceptionFilter vive en Api/Filters y continúa registrado por AddObjectsApi.
Lo utilizan CombinedImportController, FileJobController y FileJobWorkerController.
No es una biblioteca transversal: reconoce EF/MinIO y códigos FILE_JOB_* propios
de Objects. Moverlo al Core común obligaría a acoplarlo a proveedores y mensajes de
este servicio. Sus respuestas se conservan:

| Fallo | Comportamiento |
| --- | --- |
| Cancelación con RequestAborted activado | No la sustituye por una respuesta 500 |
| BadHttpRequestException por tamaño | 413 con ProblemDetails |
| UnauthorizedAccessException | 403 con FILE_JOB_FORBIDDEN |
| EF/SQL, MinIO, HTTP, I/O o timeout de tarea, también como causa interna | 503 con FILE_JOB_DEPENDENCY_UNAVAILABLE |
| Otro fallo | 500 con mensaje seguro y traceId |

No registra la excepción, su mensaje, cuerpos o credenciales: solo el tipo y la
traza. El filtro global ResultFilter del framework conserva Result y ProblemDetails.
No se cambiaron rutas, políticas, límite de cuerpo, reglas de negocio o settings.

## Verificación

CombinedImportHttpContractTests ejecuta el controller con sus perfiles de mapping,
AddObjectsApi y los filtros MVC reales. Comprueba autoridad de la ruta frente a
body/query, paginación, páginas vacías, sobres 202, ApiExplorer, validación 400,
conflicto 409 y traducción segura de excepciones 403/413/500/503.

El despachador y las políticas de esos fixtures son dobles controlados; no prueban
IAM, SQL o una sesión real en DEV. La suite existente conserva por separado sus
pruebas con MySQL y RabbitMQ descartables. No se requiere migración ni instalación
administrativa para este cambio de organización y documentación.
