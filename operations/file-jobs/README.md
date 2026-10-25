# Trabajos de archivos de Objects

Las guías canónicas pertenecen a `church-ropabajo/docs`, carpeta `naza/backend`:
`db-object-desarrollo.md`, `piloto-archivos.md` y `generacion-archivos.md`.

El usuario ya creó `db_object` y confirmó `usr_object` en `161.132.54.155:3306`.
No repetir esa creación. El control SQL está **al final** de
`docker-kubernetes-configs/docker-compose/mysql-db-object/db-init/init.sql`.
Se retiró la copia `001_metadata_dev.sql`; modificar el esquema en su fuente canónica.

El CI obtiene ese repositorio en un SHA fijo y pasa la ruta del init mediante
`OBJECTS_SCHEMA_INIT_SQL`. Las pruebas extraen solo el bloque de las seis tablas
`tarea_archivo*`, sobre MySQL 5.7.44 efímero en `127.0.0.1:13316`. No ejecutan el
resto del init, sus cuentas ni sus catálogos. Ver `FileJobLifecycleTests`.

El código Local y el operador común utilizan `mysql-local:3306`. Los Secrets reales
deben comprobarse antes de desplegar; cambiar Git no sustituye el runtime histórico
de `3307`. El arranque Local rechaza ese destino incluso con los trabajos apagados.
`FileJobs.Enabled` y `FileWorker.Enabled` permanecen apagados.
El archivo `rabbitmq-development.json` define la topología del piloto; no reemplazar
las definiciones completas del broker ni incluir credenciales en él.
