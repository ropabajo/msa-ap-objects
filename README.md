# msa-ap-objects

# ejecutar desde infraestructura
dotnet ef dbcontext scaffold "server=localhost;user=usr_object;password=pss_object;database=db_object" Pomelo.EntityFrameworkCore.MySql --output-dir ..\Ropabajo.Church.Sanluis.Objects.Domain\Entities --context-dir Persistence -c DatabaseContext --force --project ..\Ropabajo.Church.Sanluis.Objects.Infraestructure\Ropabajo.Church.Sanluis.Objects.Infraestructure.csproj --startup-project ..\Ropabajo.Church.Sanluis.Objects.Api\Ropabajo.Church.Sanluis.Objects.Api.csproj