# Build context: root repozytorium
# docker build -f Deployment/Docker/Dockerfiles/IntegratorAI.DbMigrator.Dockerfile -t integratorai-db-migrator:local .

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy source files (DbMigrator + embedded SQL scripts)
COPY Database/ Database/

# Restore NuGet packages (cache mount persists packages across builds)
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore Database/IntegratorAI.DbMigrator/IntegratorAI.DbMigrator.csproj

# Build and publish
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish Database/IntegratorAI.DbMigrator/IntegratorAI.DbMigrator.csproj \
	-c Release \
	-o /app/out \
	--no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

COPY --from=build /app/out ./

LABEL maintainer="Wiktor Wijata"
LABEL description="IntegratorAI.DbMigrator - Database migration tool"

RUN useradd --no-create-home --shell /bin/false appuser && chown -R appuser /app

ENTRYPOINT ["dotnet", "IntegratorAI.DbMigrator.dll"]
