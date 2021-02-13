ARG BUILD_FROM

FROM $BUILD_FROM AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY hass-actron/hass-actron.csproj hass-actron/
RUN dotnet restore hass-actron/hass-actron.csproj
COPY . .
WORKDIR /src/hass-actron
RUN dotnet build hass-actron.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish hass-actron.csproj -c Release -o /app

FROM base AS final
RUN addgroup --system --gid 1000 dotnet && adduser --system --uid 1000 --ingroup dotnet --shell /bin/sh dotnet
WORKDIR /app
COPY --from=publish /app .
USER 1000
ENTRYPOINT ["dotnet", "hass-actron.dll"]
