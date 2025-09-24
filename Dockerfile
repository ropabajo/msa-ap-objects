FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG GITHUB_USERNAME
ARG GITHUB_TOKEN

RUN dotnet nuget add source https://nuget.pkg.github.com/robertpablo/index.json -n GitHub -u ${GITHUB_USERNAME} -p ${GITHUB_TOKEN} --store-password-in-clear-text

COPY . .
RUN dotnet restore "src/Ropabajo.Church.Sanluis.Objects.Api/Ropabajo.Church.Sanluis.Objects.Api.csproj" \
 # Limpia el feed y el archivo de config para que el PAT no quede en la capa
 && dotnet nuget remove source GitHub \
 && rm -f /root/.nuget/NuGet/NuGet.Config

FROM build AS publish
RUN dotnet publish "src/Ropabajo.Church.Sanluis.Objects.Api/Ropabajo.Church.Sanluis.Objects.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Ropabajo.Church.Sanluis.Objects.Api.dll"]