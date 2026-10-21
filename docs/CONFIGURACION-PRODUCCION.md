# Configuración de producción

Contrato público: `src/Ropabajo.Church.Sanluis.Objects.Api/appsettings.Production.json`.
Este candidato conserva `appsettings.Local.json` y sus dependencias de church-dev.

- `ASPNETCORE_ENVIRONMENT=Production` y `DOTNET_ENVIRONMENT=Production` seleccionan este contrato.
- `ASPNETCORE_URLS=http://+:8080` publica HTTP interno detrás de Nginx/TLS.
- `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` evita observadores de archivos; los cambios requieren rollout.
- Config Server queda desactivado por `spring.cloud.config.enabled=false` en el fuente.
- El namespace y contexto destino son `ropabajo-church-sanluis` y `minikube`.
- MySQL usa `mysql-objects` (Service de puerto 3306). El EndpointSlice conserva la instancia existente; no se importa información de desarrollo.
- Redis usa el Service productivo, DB **0**; Local conserva DB 1. No son fronteras ACL entre ambientes.
- Los secretos sobrescriben campos vacíos mediante variables con separador `__`; nunca se incorporan a Git ni a la imagen.

La imagen debe incluir únicamente `appsettings.json` público y `appsettings.Production.json`,
con las mismas claves y valores públicos que el fuente. Los campos privados vacíos deben
ser satisfechos por el Secret de runtime antes del despliegue. No inyectar `GITHUB_TOKEN`
ni `GITHUB_USERNAME` en el pod; las credenciales de paquetes son solo de compilación.

Authz 1.1.4 se compila desde la revisión fijada del framework, con pruebas y procedencia;
no usar la versión prerelease ni restaurar una biblioteca diferente durante el corte.

## Mensajería

Se conserva `dev-vhost`: es el vhost **real de producción**, confirmado por lectura del
runtime el 20 de octubre de 2026. No renombrarlo por su nombre ni conectarlo a
`local-vhost`, que pertenece a church-dev. Conservar contratos de eventos, bindings y
consumidores antes de activar escrituras.
