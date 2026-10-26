# Instrucciones del repositorio

## Configuración del ambiente Local

- Toda configuración de aplicación implementada en Kubernetes debe reflejarse en
  el fuente del mismo ambiente, con las mismas claves, tipos y valores públicos.
  En church-dev usar `appsettings.Local.json`; conservar producción sin cambios.
- Mantener credenciales vacías en el fuente y la imagen; los valores reales se
  inyectan desde Secrets. Derivar los defaults públicos de CI desde el fuente.
- Documentar la precedencia y las variables propias de .NET/ASPNETCORE en
  `docs/CONFIGURACION-LOCAL.md`. No añadir flags de integraciones no utilizadas.
- Antes de publicar cambios de configuración, ejecutar el auditor común de
  `k8s-recipes/environments/church-dev/operations/audit-local-settings.sh`, con los
  ocho checkouts hermanos y la revisión de recetas fijada por cada workflow.

## Estándar de backend Naza

- Aplicar la guía canónica de `church-ropabajo/docs`, `naza/backend/lineamientos-refactor.md`, a los casos nuevos o intervenidos.
- En este workspace está en `/workspace/docs/naza/backend/lineamientos-refactor.md`; leerla antes de ampliar el caso de uso.
- Casos separados en Commands/Queries; DTOs propios, MappingProfile, validator, documentación XML completa y CancellationToken.
- Los casos nuevos utilizan Result de ap-framework. Conservar contratos legacy y evitar dos canales de error en el caso migrado.
- No ampliar por arrastre el alcance a otros casos ni modificar producción al implementar Local.

## RabbitMQ y Program.cs

- Mensajería nueva sigue la sección 13 de los lineamientos canónicos de Naza:
  IProducer<T>, productor concreto en Infrastructure y transporte confirmado del framework.
- No duplicar canales, ACK, retry o reconexión en el MSA. Reutilizar la sección RabbitMq del MSA y fijar una revisión inmutable del framework en CI.
- Program.cs compone capas y middleware; filtros/registros propios van en su capa,
  y el comportamiento reutilizable de autenticación o AMQP en Authz/RabbitMq.

## Nombres y conexión RabbitMQ

- Usar una sola sección RabbitMq por microservicio. No agregar FileJobs.Broker,
  FileWorker.Broker, Queue o Exchange en appsettings para los nombres de mensajería.
- Exchange/routing key se definen en el productor; QueueName en el consumidor,
  siguiendo nc/ne/nb y el nombre del servicio. El ambiente lo separa VirtualHost.
- Registrar sus bindings en las definiciones RabbitMQ existentes de k8s-recipes;
  no crear otra fuente de topología en el MSA. No duplicar credenciales por feature.
