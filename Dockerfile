# API REST de FarmaciaApp. Construir desde la raíz del repositorio:
#   docker build -t farmacia-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json ./
COPY src/FarmaciaApp.Core/*.csproj src/FarmaciaApp.Core/
COPY src/FarmaciaApp.Infrastructure/*.csproj src/FarmaciaApp.Infrastructure/
COPY src/FarmaciaApp.Api/*.csproj src/FarmaciaApp.Api/
RUN dotnet restore src/FarmaciaApp.Api

COPY src/FarmaciaApp.Core src/FarmaciaApp.Core
COPY src/FarmaciaApp.Infrastructure src/FarmaciaApp.Infrastructure
COPY src/FarmaciaApp.Api src/FarmaciaApp.Api
RUN dotnet publish src/FarmaciaApp.Api -c Release --no-restore -p:ContinuousIntegrationBuild=true -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    TZ=America/Bogota
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "FarmaciaApp.Api.dll"]
