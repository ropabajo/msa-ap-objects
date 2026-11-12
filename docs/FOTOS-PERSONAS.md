# Validación de fotos antes de confirmar la carga

Desplegado en desarrollo el 12/11/2026. No se modifica producción, secretos ni permisos.

## Reglas del servidor

Para la carpeta `photos`, Objects permite JPG/JPEG, PNG y WebP, como máximo
5 × 1024 × 1024 bytes y 24.000.000 píxeles. El usuario confirmó el límite de
24 megapíxeles. La reserva impone su lista y tamaño propios, aunque el cliente
solicite más tamaño u otras extensiones. Normaliza la extensión y la carpeta.
Las políticas firmadas conservan el rango de contenido y el MIME obligatorio.

Antes de marcar `state/uploaded`, Objects consulta el objeto con su identidad
MinIO existente, comprueba tamaño y MIME, lee como máximo 5 MB y comprueba el
ETag de la lectura. SkiaSharp decodifica el fotograma mostrado como foto y exige
que corresponda al formato declarado. Una firma o extensión por sí sola no basta.
Se rechazan archivos vacíos, dañados, incompletos, incompatibles o de resolución
excesiva. La validación se serializa por instancia y limita la resolución antes
de reservar memoria para el bitmap. Se usa SkiaSharp 3.119.4, con activos Linux
sin dependencias de sistema; no se usa System.Drawing ni una licencia comercial.

No se abre una transacción de negocio durante la lectura de almacenamiento.
Sólo después del resultado válido se confirman estado e historial juntos.
Si falla, la web no debe enviar el guardado de la ruta a Personas. Personas
conserva la validación de su URL permanente, host, bucket y carpeta; no recibe
el archivo binario. No se añaden llamadas HTTP entre Personas y Objects.

## Compatibilidad

Se intervienen sólo reserva y confirmación genéricas, conservando rutas, nombres
de campos JSON, cabeceras `url`/`sanluis-object-code`/`x-form-data-*`, CORS y éxitos
201/200 sin cuerpo. Los casos usan DTO, mapas y Result; los errores normalizados
mantienen `errors[]` con código y mensaje seguro. El filtro HTTP real está probado.
Los formatos que no son fotos siguen su autorización de carga anterior; no se
aplica el decodificador de imagen a una planilla Excel.

No se migran bases ni se eliminan archivos o fotos anteriores. Una carga rechazada
puede dejar el objeto recibido sin asociar; su limpieza es una función separada.
Las autorizaciones firmadas conservan su vigencia; esta comprobación acredita
el contenido leído al confirmar, no una garantía de inmutabilidad futura del objeto.

## Pruebas

- Decodificador real de JPG/JPEG, PNG y WebP; vacío, exceso de 5 MB, MIME falso,
  extensión discordante, truncamiento y resolución declarada superior a 24 MP.
- SDK MinIO real contra HTTP local: HEAD/GET, rechazo por tamaño antes de leer,
  decodificación de contenido y límites de la política firmada.
- Casos de uso: un cliente no puede ampliar la política de fotos y una imagen
  rechazada no modifica estado ni historial. Se mantiene la protección de rutas
  reservadas para exportaciones.
- HTTP del controller real con ResultFilter: cabeceras, cuerpo vacío, identificador
  de ruta y respuesta de validación. La autenticación de ese fixture es local y
  aislada, sin tokens ni usuarios reales; no acredita permisos de un usuario vivo.

## Resultado en desarrollo

El [pipeline 43255893314](https://github.com/ropabajo/msa-ap-objects/actions/runs/43255893314)
ejecutó correctamente 95 pruebas de Objects, además de las comprobaciones de sus
dependencias. Desplegó `dev-objects` con digest
`sha256:b31ded2604bc61769f372f493441471fd5d25c5523c9f2e25b707cb30feccc72`.
El operador común confirmó rollout, salud HTTPS y disponibilidad de producción.
La prueba del navegador utiliza HTTP simulado; queda pendiente una carga autenticada
con un usuario de desarrollo en el ambiente publicado y cualquier pase a producción.
