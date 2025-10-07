FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5066

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_TOKEN

COPY ["NotificationService.API/NotificationService.API.csproj", "NotificationService.API/"]
COPY ["NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "NotificationService.Infrastructure/"]
COPY ["NotificationService.Domain/NotificationService.Domain.csproj", "NotificationService.Domain/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet nuget add source https://nuget.pkg.github.com/oldboni22/index.json -u oldboni22 -p ${GITHUB_TOKEN} --store-password-in-clear-text
RUN dotnet restore "NotificationService.API/NotificationService.API.csproj"

COPY . .
WORKDIR "/src/NotificationService.API"
RUN dotnet build "./NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
