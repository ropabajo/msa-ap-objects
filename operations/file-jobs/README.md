# Trabajos de archivos: infraestructura DEV

La conexión AMQP es la sección RabbitMq existente del MSA; no hay un Broker por
feature ni usuarios Rabbit adicionales. Las credenciales siguen en el Secret
actual de la aplicación, sin copiarlas a dev-file-jobs-*.

Los nombres los fijan FileJobProducer y ChurchFileJobConsumer. Las definiciones
viven en `k8s-recipes/rabbit-mq/definitions-configmap.yaml`, como el resto del
proyecto. Se reutiliza nc.objects.exchange (direct) en local-vhost:

| Ruta | Cola |
| --- | --- |
| nc.objects.file-jobs.church.requested | nc.churches.file-jobs.queue |
| nc.objects.file-jobs.church.retry | nc.churches.file-jobs.retry.queue |
| nc.objects.file-jobs.church.failed | nc.churches.file-jobs.dead.queue |

El retry tiene TTL de 30 s y vuelve a requested; la principal rechaza hacia failed.
La cola específica no compite con el consumidor existente de nc.churches.queue.
Se retiró el JSON paralelo rabbitmq-development.json del MSA.

Antes de activar DEV, verificar los bindings y permisos de la cuenta RabbitMq
actual sobre esas rutas dentro de local-vhost. Incorporar solo esos recursos con
el procedimiento habitual del broker, con respaldo/recuperación previos; no
reemplazar definiciones completas ni modificar otros vhosts. Los aplicativos no
administran topología y los flags siguen apagados.

Secret de Objects: ConnectionStrings__FileJobs. Secret del worker de Church:
FileWorker__ClientSecret. El cliente técnico Keycloak y audiencia objects-files
se conservan para llamadas HTTP; Authority se toma de Keycloak.Authority.
No hay credenciales Rabbit nuevas en estos Secrets.
