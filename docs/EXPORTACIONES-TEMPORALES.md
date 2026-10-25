> Documento de compatibilidad del prototipo anterior. El estándar para nuevos módulos es [archivos durables](../operations/file-jobs/README.md). No extender el protocolo de filas JSON; conservarlo únicamente durante el corte coordinado.

# Exportaciones XLSX privadas y temporales

El servicio existente Objects genera los Excel de miembros preparados por Church.
Los controllers solo adaptan HTTP a casos de uso de Application; Infrastructure
implementa MinIO, XLSX, bloqueos y el worker. No se agregan tablas ni un servicio nuevo.

## Plazos públicos de Local

Valores en `appsettings.Local.json`, preservados por el saneamiento de la imagen:

| Propiedad de TemporaryExports | Valor | Función |
| --- | --- | --- |
| Enabled | true | Activa rutas/reconciliación en Local; ausente equivale a false |
| RetentionHours | 24 | Disponibilidad desde generación, independiente de la firma |
| ReuseSeconds | 300 | Antigüedad máxima de datos reutilizables; pendiente siempre se comparte |
| LinkSeconds | 900 | Firma máxima de descarga; limitada por la vigencia restante |
| InputTimeoutSeconds | 120 | Plazo para recibir el JSON inicial |
| PollSeconds | 5 | Intervalo del worker |
| BatchSize | 25 | Trabajos procesados por recorrido; cursor entre recorridos |
| MaxAttempts | 3 | Intentos de generación antes de marcar failed |
| MaxRows / MaxColumns | 10000 / 40 | Límite sin truncar datos |
| MaxSourceBytes | 10485760 | Máximo JSON de fuente: 10 MiB; también límite HTTP |

Las credenciales MinIO existentes provienen de `dev-objects-runtime`; el bucket
es privado `church-dev`. No se copian claves a Church ni a la web. No se crean
credenciales, ACL de Redis, migraciones ni políticas globales de bucket.

## Persistencia y exclusión entre réplicas

Único prefijo administrado: `temporary-exports/v1/`.

```text
temporary-exports/v1/jobs/{church}/{ownerHash}/{exportId}/manifest.json
temporary-exports/v1/jobs/{church}/{ownerHash}/{exportId}/source.json
temporary-exports/v1/jobs/{church}/{ownerHash}/{exportId}/document.xlsx
temporary-exports/v1/indexes/{church}/{ownerHash}/active-{fingerprint}.json
temporary-exports/v1/indexes/{church}/{ownerHash}/request-{requestId}.json
```

`ownerHash` deriva de SHA-256(emisor + separador + subject) del token validado,
no del cuerpo ni del nombre público. El fingerprint incluye tipo/versión del
reporte y filtros normalizados del productor. Cada usuario tiene su propia búsqueda.

MySQL `GET_LOCK(nombre,0)` en la conexión existente de Objects protege intención,
búsqueda y trabajo entre réplicas. Usa conexiones dedicadas no agrupadas; al
cerrarlas se libera el bloqueo. No escribe filas ni crea esquema. El nombre incluye
endpoint/bucket y SHA-256 de la clave, acotado a 64 caracteres. Un bloqueo ocupado
responde 409/reintento, sin crear archivos duplicados. Los archivos listos no abren
bloqueos en cada recorrido; generación y limpieza sí los requieren.

El manifiesto reserva un único UUID. Una vez persistida `source.json`, el worker
puede recuperarse tras reinicio, renderizar y escribir `document.xlsx` antes de
publicar ready. Después retira la fuente; si ese retiro falla, el vencimiento
posterior también la limpia. No se guarda el token del usuario.

El mismo RequestId es idempotente durante su retención y no admite otros filtros.
Una intención nueva reutiliza archivos listos durante cinco minutos; Actualizar
solicita una nueva versión. Trabajos pendientes nunca se duplican. Tras tres
intentos fallidos se publica failed y una nueva intención puede reemplazarlos.

## API JSON y seguridad

Rutas bajo `/v1/church/{churchCode}/exports`, política `ActiveChurchRoute`:

| Método/ruta relativa | Cuerpo / respuesta |
| --- | --- |
| POST raíz | `{requestId, fingerprint, kind, forceRefresh}` → 202 estado y needsSource |
| PUT `/{id}/source` | `{title, columns, rows, dataAsOf}` → 202 estado |
| GET `/{id}` | Estado; no crea trabajos ni devuelve el JSON de personas |
| POST `/{id}/download` | `{url, expiresAt}` para el XLSX ya listo |

Ejemplo de columna: `{"title":"Fecha de bautismo","kind":"date","width":23}`.
Tipos admitidos: text, date (`yyyy-MM-dd`) e integer. Una celda null queda vacía;
no se aceptan fórmulas/macros. El XLSX lleva fecha de consulta y emisión en Perú.
El nombre de descarga se genera en el servidor; nunca usa texto libre como ruta.

Solo el mismo usuario y la misma iglesia pueden consultar, aportar o firmar su
trabajo. Un exportId ajeno/no existente devuelve 404; vencido 410; pendiente/ocupado
409; inválido 422; demasiado grande 413. Fallas de proveedor devuelven 503 sin
imprimir tokens, fuentes, URLs firmadas o mensajes internos en logs.
Los endpoints genéricos de subida/firma rechazan `temporary-exports/`: no permiten
eludir la autorización o renovar la firma de un archivo vencido.

La descarga se realiza **directamente desde MinIO** con Content-Type XLSX y
Content-Disposition attachment. Una firma permite descargar a quien posea ese
enlace durante su vigencia; no registrarla ni difundirla. Renovar firma no vuelve a
generar el archivo ni prolonga su retención. La copia ya descargada no vence.

## Limpieza y recuperación

La disponibilidad lógica termina exactamente al vencer el manifiesto. Después,
el worker elimina XLSX, fuente residual, manifiesto e índices vencidos. Es una
limpieza de aplicación bajo el único prefijo temporal, **no** una política de
ciclo de vida global de MinIO. No toca `photos/`, cargas masivas ni PDFs electorales.
La eliminación usa los mismos bloqueos que reserva/generación y revalida los índices
para no borrar uno renovado concurrentemente. Un trabajo corrupto no bloquea al resto.

Con Objects detenido, pueden quedar objetos físicos vencidos; no se vuelven a
firmar al arrancar y el worker recupera limpieza/trabajos. La demora de limpieza
depende de cantidad de manifiestos, BatchSize, PollSeconds y disponibilidad de SQL/MinIO.
No repetir instalaciones de runtime, ACL o reinicios para recuperar esta cola.

## Entrega y validación

Orden: **Objects → Church → web-church**, mediante workflows DEV existentes.
Local/CI incluyen todos los nuevos valores públicos desde el fuente. No se
modifican settings ni workflows de producción. No es necesario modificar Nginx.

`dotnet test src/Ropabajo.Church.Sanluis.Objects.Api.Tests` cubre reutilización,
intenciones, dos réplicas, reinicio, vencimiento, limpieza concurrente, límites,
propietario/iglesia, contrato HTTP, SDK S3 y XLSX real. El workflow ejecuta esa suite
y conserva TRX junto con las pruebas compartidas de Authz. El servidor S3 de prueba
es un fixture de protocolo; no acredita conectividad al MinIO del servidor.

Comprobar después del rollout con un usuario autorizado de DEV: varias páginas
sin truncar, diez solicitudes equivalentes con un solo exportId, renovación que
mantenga id/dataAsOf, Actualizar datos que cambie id y rechazo entre usuarios/iglesias.
Ver [productor Church](https://github.com/church-ropabajo/msa-church/blob/development/docs/EXPORTACION-MIEMBROS.md).
