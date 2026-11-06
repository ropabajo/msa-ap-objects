# Promoción de carga masiva y exportaciones

El candidato integra el flujo validado en desarrollo con la configuración
Production existente. El pase completo fue autorizado por el usuario el
31/10/2026 (America/Lima). Publicar una imagen o integrar main no equivale a
desplegarla: el corte se ejecuta con la relación nueva de release en k8s-recipes.

Se conservan los datos productivos, namespace ropabajo-church-sanluis, dos réplicas
y dominios idnsanluis.org/auth.idnsanluis.org. La configuración pública reside en
el aplicativo; contraseñas y tokens se inyectan desde Secrets y no se registran.
Los destinos DEV, configuración Local y ramas publicadas se preservan.

El paquete se construye desde commits inmutables: framework
3e8a19272f9fc75ee94fe553f976c34adba6ae15 y shared
eaa1d5070797d3913c45be6e7cbae09318337f36 cuando lo requiere el aplicativo. Las
bibliotecas nuevas conservan las versiones exactas probadas, incluidas etiquetas
pre-release; es una excepción documentada para este candidato congelado. No se
republica otra implementación bajo una versión estable existente. El build
registra las versiones efectivas y el hash de settings públicos de su imagen.

La promoción exige respaldo restaurado, migración incremental española de las
tres bases participantes, dependencias técnicas comprobadas, ensayo y recuperación.
No ejecutar init.sql sobre bases existentes ni repetir el corte histórico de octubre.
La información de lotes y filas pertenece a Objects; Personas y Church conservan
sólo recepción/salida local y la versión de réplica en persona.VERSION_ORIGEN.

Las exportaciones usan el bucket privado ropabajo y las identidades productivas.
Objects conserva la reutilización de cinco minutos por filtros/usuario/iglesia y
las firmas temporales. El avance de importación mantiene REST y WebSocket
autorizados; las fechas de registro y actualización viajan en UTC explícito.

La relación productiva de fuentes, imágenes, respaldos, pruebas y recuperación se
publica en k8s-recipes/environments/production/releases/bulk-exports/. Los estados
de build/ensayo/corte se registran por separado; no dar por realizada la prueba
humana del portal basándose sólo en una suite o un health.

La integración confirmada, consumidores y mantenimiento se activan en Local y
Production. Otros ambientes no exponen el contrato combinado por esa regla.
La limpieza conserva pendientes/fallidos y aplica la retención ya probada de
treinta días a correlaciones cerradas, cada hora y en lotes acotados.

FileJobs Production usa db_object y el alias mysql-objects del namespace
productivo, con una cuenta exclusiva de sus tablas. DefaultConnection y FileJobs
deben coincidir en servidor, puerto y base; no se crea otra base de metadatos.
