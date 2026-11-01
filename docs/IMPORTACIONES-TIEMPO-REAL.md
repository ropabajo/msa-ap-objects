# Avance de importaciones en Local

El listado y el detalle reciben contadores confirmados mediante SignalR sobre
WebSocket. El worker conserva su protocolo RabbitMQ y sus transacciones actuales:
una fila termina cuando Church confirma Miembro después de Persona. El canal no
crea tablas, eventos, clientes Keycloak ni secciones de configuración.

## Contrato y permisos

1. `POST /v2/churches/{churchCode}/imports/progress-ticket`, con el JWT y
   `X-Church-Code` habituales, aplica `IamAdmin` y `ActiveChurchRoute`, igual que
   la bandeja. La respuesta Result contiene `value.ticket` y `value.expiresAt`;
   `Cache-Control: no-store`. No recibe identidad o iglesia desde el body.
2. Conectar a `/v2/churches/{churchCode}/imports/progress?ticket=<nonce>` mediante
   WebSocket, sin negociación/long polling. El nonce aleatorio de un uso caduca
   en 20 segundos y exige el mismo Origin/ruta. El JWT no aparece en la URL.
3. Invocar `Subscribe` con `{ "loadCodes": ["<guid>"] }`, máximo 100 distintos.
   La autoridad procede exclusivamente del ticket preparado por el servidor.
   Si una carga no pertenece al usuario/iglesia, rechaza la suscripción completa
   sin distinguir una carga inexistente de una ajena.
4. `ProgressChanged` entrega una lista con `bulkLoadCode`, `status`, `stateCode`,
   `records`, `uploadedRecords`, `observedRecords`, `updatedAt` UTC y `version`.
   No entrega filas, datos personales ni enlaces firmados.

Objects observa cada dos segundos los contadores **ya confirmados** en MySQL,
solo mientras hay conexiones suscritas. Envía únicamente versiones nuevas, no
un evento por fila. Cada consulta/envío tiene un plazo acotado. Si falla, cierra
el canal para que la reconexión recupere una instantánea actual. El navegador
conserva filtros, paginación y selección; ignora versiones atrasadas y vuelve a
consultar tras conectar. Mientras recupera el canal consulta por REST cada 10 s.
Al abandonar la página cancela HTTP, temporizadores y socket.

La sesión dura como máximo cinco minutos, acotada además por la expiración del
JWT. La reconexión exige un ticket nuevo y repite las políticas HTTP/IAM. Una
revocación de permisos se vuelve a comprobar al renovar; la ventana máxima de
una sesión existente es cinco minutos. Límites: 128 conexiones por proceso, tres
por propietario/iglesia, mensajes entrantes de 8192 bytes. No habilitar otra
réplica sin un almacén de tickets compartido/afinidad y un backplane SignalR.

La parte técnica reutilizable está en ap-framework `Realtime` 1.0.0-dev.1.
`Program.cs` solo compone `UseImportProgress` antes de autenticación y mapea el hub.
El registro, controlador y hub se publican exclusivamente en `Local`.

## Activación y pruebas

El proxy público debe conservar HTTP/1.1, Upgrade y Connection en `/ap-objects/`.
La propuesta acotada está en docker-kubernetes-configs,
`docker-compose/nginx/development/dev.idnsanluis.portal-imports-realtime-dns.conf`.
El perfil `portal-imports-realtime` de `activate-core-apis.sh` acepta solamente el
vhost de imports conocido o el candidato exacto; respalda, verifica `nginx -t`,
hace reload y revierte ante fallos. Cambiar el Nginx compartido requiere la
autorización específica establecida por el usuario. Publicar código/imagen no
prueba que WebSocket esté activado en la URL pública.

Pruebas: tickets (expiración, copia, concurrencia, consumo y capacidad), HTTP y
WebSocket reales con TestServer (políticas, Origin/ruta, replay, límites,
propietario/iglesia, reconexión), lector sobre MySQL desechable, pruebas Angular
de recuperación/destrucción/versiones, y proxy Nginx real aislado con upgrade101.
Ninguna prueba crea personas o miembros reales ni usa credenciales productivas.
