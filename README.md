# msa-ap-objects

# ejecutar desde infraestructura
dotnet ef dbcontext scaffold "server=localhost;user=usr_object;password=pss_object;database=db_object" Pomelo.EntityFrameworkCore.MySql --output-dir ..\Ropabajo.Church.Sanluis.Objects.Domain\Entities --context-dir Persistence -c DatabaseContext --force --project ..\Ropabajo.Church.Sanluis.Objects.Infraestructure\Ropabajo.Church.Sanluis.Objects.Infraestructure.csproj --startup-project ..\Ropabajo.Church.Sanluis.Objects.Api\Ropabajo.Church.Sanluis.Objects.Api.csproj


## este comando debe generar la imagen
```sh
docker compose build
```

## Etiquetar la imagen para Docker Hub:
pasa de tal nombre a otro 
```sh
docker tag msa-ap-objects-be:latest ropbajo/msa-ap-objects:v1.0
```

## lo sube a docker-hub
```sh
docker push ropbajo/msa-ap-objects:v1.0
```

## cambien el nombre de la cadena de conexión despues de generar la imagen v1.0 -- revisar al desplegar en el minikube


## para acceder a la virtual
ssh root@161.132.54.155
