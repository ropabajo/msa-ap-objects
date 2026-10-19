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
