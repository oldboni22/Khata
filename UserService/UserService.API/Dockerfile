FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7082
EXPOSE 5100

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

ARG GITHUB_TOKEN

COPY ["UserService.API/UserService.API.csproj", "UserService.API/"]
COPY ["UserService.BLL/UserService.BLL.csproj", "UserService.BLL/"]
COPY ["UserService.DAL/UserService.DAL.csproj", "UserService.DAL/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet nuget add source https://nuget.pkg.github.com/oldboni22/index.json -u oldboni22 -p ${GITHUB_TOKEN} --store-password-in-clear-text

RUN dotnet restore "UserService.API/UserService.API.csproj"
COPY . .
WORKDIR "/src/UserService.API"
RUN dotnet build "./UserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.API.dll"]
