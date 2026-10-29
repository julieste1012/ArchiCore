# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./global.json
COPY src/Backend/ ./src/Backend/

RUN dotnet restore src/Backend/ArchiCore.API/ArchiCore.API.csproj
RUN dotnet publish src/Backend/ArchiCore.API/ArchiCore.API.csproj \
    --configuration Release \
    --no-restore \
    --output /out/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/App_Data/Objects \
    && chown -R $APP_UID:$APP_UID /app/App_Data

COPY --from=build /out/publish/ ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "ArchiCore.API.dll"]
