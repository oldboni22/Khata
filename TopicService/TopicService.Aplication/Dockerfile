FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7195
EXPOSE 5200

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_TOKEN

COPY ["TopicService.Aplication/TopicService.Aplication.csproj", "TopicService.Aplication/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet nuget add source https://nuget.pkg.github.com/oldboni22/index.json -u oldboni22 -p ${GITHUB_TOKEN} --store-password-in-clear-text
RUN dotnet restore "TopicService.Aplication/TopicService.Aplication.csproj"

COPY . .
WORKDIR "/src/TopicService.Aplication"
RUN dotnet build "./TopicService.Aplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./TopicService.Aplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TopicService.Aplication.dll"]
