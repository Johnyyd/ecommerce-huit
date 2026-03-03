# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ECommerce.Huit.API/ECommerce.Huit.API.csproj", "ECommerce.Huit.API/"]
COPY ["ECommerce.Huit.Application/ECommerce.Huit.Application.csproj", "ECommerce.Huit.Application/"]
COPY ["ECommerce.Huit.Domain/ECommerce.Huit.Domain.csproj", "ECommerce.Huit.Domain/"]
COPY ["ECommerce.Huit.Infrastructure/ECommerce.Huit.Infrastructure.csproj", "ECommerce.Huit.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ECommerce.Huit.API/ECommerce.Huit.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ECommerce.Huit.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install wait-for-it script (optional for docker-compose dependencies)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "ECommerce.Huit.API.dll"]
