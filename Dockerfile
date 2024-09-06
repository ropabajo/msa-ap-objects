FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src

ARG GITHUB_USERNAME
ARG GITHUB_TOKEN

RUN dotnet nuget add source https://nuget.pkg.github.com/robertpablo/index.json -n GitHub -u ${GITHUB_USERNAME} -p ${GITHUB_TOKEN} --store-password-in-clear-text

COPY . .
RUN dotnet restore "src/Ropabajo.Church.Sanluis.Objects.Api/Ropabajo.Church.Sanluis.Objects.Api.csproj"

FROM build AS publish
RUN dotnet publish "src/Ropabajo.Church.Sanluis.Objects.Api/Ropabajo.Church.Sanluis.Objects.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=publish /app/publish .

RUN update-ca-certificates
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
ENTRYPOINT ["dotnet", "Ropabajo.Church.Sanluis.Objects.Api.dll"]