# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER $APP_UID

WORKDIR /app

EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["TimePlanner.slnx", "."]
COPY ["TimePlanner.API/TimePlanner.API.csproj", "TimePlanner.API/"]
COPY ["TimePlanner.Domain/TimePlanner.Domain.csproj", "TimePlanner.Domain/"]
COPY ["TimePlanner.Repositories/TimePlanner.Repositories.csproj", "TimePlanner.Repositories/"]
COPY ["TimePlanner.Services/TimePlanner.Services.csproj", "TimePlanner.Services/"]
COPY ["TimePlanner.Tests/TimePlanner.Tests.csproj", "TimePlanner.Tests/"]

RUN dotnet restore "TimePlanner.slnx"

COPY . .

# This stage runs the unit tests before the API is published
FROM build AS test

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

RUN dotnet test "./TimePlanner.Tests/TimePlanner.Tests.csproj" \
    -c $BUILD_CONFIGURATION \
    --no-restore

# This stage publishes the API only after all unit tests pass
FROM test AS publish

ARG BUILD_CONFIGURATION=Release

WORKDIR "/src/TimePlanner.API"

RUN dotnet publish "./TimePlanner.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimePlanner.API.dll"]