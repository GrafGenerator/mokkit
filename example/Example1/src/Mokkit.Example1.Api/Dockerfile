# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["Directory.Packages.props", "."]
COPY ["src/Mokkit.Example1.Api/Mokkit.Example1.Api.csproj", "src/Mokkit.Example1.Api/"]
COPY ["src/Mokkit.Example1.Application/Mokkit.Example1.Application.csproj", "src/Mokkit.Example1.Application/"]
COPY ["src/Mokkit.Example1.Common/Mokkit.Example1.Common.csproj", "src/Mokkit.Example1.Common/"]
COPY ["src/Mokkit.Example1.Domain/Mokkit.Example1.Domain.csproj", "src/Mokkit.Example1.Domain/"]
COPY ["src/Mokkit.Example1.Db.Postgres/Mokkit.Example1.Db.Postgres.csproj", "src/Mokkit.Example1.Db.Postgres/"]
COPY ["src/Mokkit.Example1.Infrastructure/Mokkit.Example1.Infrastructure.csproj", "src/Mokkit.Example1.Infrastructure/"]

# Restore packages
RUN dotnet restore "src/Mokkit.Example1.Api/Mokkit.Example1.Api.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/Mokkit.Example1.Api"
RUN dotnet build "Mokkit.Example1.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Mokkit.Example1.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Run as the non-root user the .NET base image already provides ($APP_UID). The old `adduser` step was
# dropped: the .NET 10 base image no longer ships `adduser`, and .NET 8+ images pre-create this user.
USER $APP_UID

# Listen on 8080 (readiness is probed over HTTP at GET /health by the orchestrator / E2E wait strategy).
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Mokkit.Example1.Api.dll"]
