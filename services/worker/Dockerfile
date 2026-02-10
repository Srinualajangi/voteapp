# =============================================================================
# Worker Service Dockerfile (.NET Core)
# =============================================================================
# Multi-stage build:
# - Stage 1: SDK image (large) to build the application
# - Stage 2: Runtime image (small) to run the application

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Security: Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Change ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# No port exposed (worker processes in background)

CMD ["dotnet", "Worker.dll"]
