# Build context: root repozytorium
# docker build -f Deployment/Docker/Dockerfiles/IntegratorAI.Api.Dockerfile -t integratorai-api:local .

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy source files
COPY Source/ Source/

# Restore NuGet packages (cache mount persists packages across builds)
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet restore Source/Integrator/IntegratorAI.Api/IntegratorAI.Api.csproj

# Build and publish
RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish Source/Integrator/IntegratorAI.Api/IntegratorAI.Api.csproj \
	-c Release \
	-o /app/out \
	--no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published app
COPY --from=build /app/out ./

# Metadata
LABEL maintainer="Wiktor Wijata"
LABEL description="IntegratorAI.Api - .NET 10 REST API"

# Run as non-root user
RUN useradd --no-create-home --shell /bin/false appuser && chown -R appuser /app

# Expose port
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "IntegratorAI.Api.dll"]
