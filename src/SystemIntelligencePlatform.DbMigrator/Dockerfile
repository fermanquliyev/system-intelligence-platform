# Build stage: build and publish DbMigrator from source so docker-compose can build without host publish.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["common.props", "."]
COPY ["src/SystemIntelligencePlatform.DbMigrator/SystemIntelligencePlatform.DbMigrator.csproj", "src/SystemIntelligencePlatform.DbMigrator/"]
COPY ["src/SystemIntelligencePlatform.EntityFrameworkCore/SystemIntelligencePlatform.EntityFrameworkCore.csproj", "src/SystemIntelligencePlatform.EntityFrameworkCore/"]
COPY ["src/SystemIntelligencePlatform.Application.Contracts/SystemIntelligencePlatform.Application.Contracts.csproj", "src/SystemIntelligencePlatform.Application.Contracts/"]
COPY ["src/SystemIntelligencePlatform.Domain/SystemIntelligencePlatform.Domain.csproj", "src/SystemIntelligencePlatform.Domain/"]
COPY ["src/SystemIntelligencePlatform.Domain.Shared/SystemIntelligencePlatform.Domain.Shared.csproj", "src/SystemIntelligencePlatform.Domain.Shared/"]
RUN dotnet restore "src/SystemIntelligencePlatform.DbMigrator/SystemIntelligencePlatform.DbMigrator.csproj"

COPY src/ ./src/
WORKDIR "/src/src/SystemIntelligencePlatform.DbMigrator"
RUN dotnet publish "SystemIntelligencePlatform.DbMigrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run stage: run once and exit. Use aspnet (not runtime) so Microsoft.AspNetCore.App is available (required by EF/ABP).
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SystemIntelligencePlatform.DbMigrator.dll"]
