FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["common.props", "."]
COPY ["src/SystemIntelligencePlatform.BackgroundWorker/SystemIntelligencePlatform.BackgroundWorker.csproj", "src/SystemIntelligencePlatform.BackgroundWorker/"]
COPY ["src/SystemIntelligencePlatform.Application/SystemIntelligencePlatform.Application.csproj", "src/SystemIntelligencePlatform.Application/"]
COPY ["src/SystemIntelligencePlatform.Application.Contracts/SystemIntelligencePlatform.Application.Contracts.csproj", "src/SystemIntelligencePlatform.Application.Contracts/"]
COPY ["src/SystemIntelligencePlatform.EntityFrameworkCore/SystemIntelligencePlatform.EntityFrameworkCore.csproj", "src/SystemIntelligencePlatform.EntityFrameworkCore/"]
COPY ["src/SystemIntelligencePlatform.Domain/SystemIntelligencePlatform.Domain.csproj", "src/SystemIntelligencePlatform.Domain/"]
COPY ["src/SystemIntelligencePlatform.Domain.Shared/SystemIntelligencePlatform.Domain.Shared.csproj", "src/SystemIntelligencePlatform.Domain.Shared/"]
RUN dotnet restore "src/SystemIntelligencePlatform.BackgroundWorker/SystemIntelligencePlatform.BackgroundWorker.csproj"
COPY src/ ./src/
WORKDIR "/src/src/SystemIntelligencePlatform.BackgroundWorker"
RUN dotnet build "SystemIntelligencePlatform.BackgroundWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemIntelligencePlatform.BackgroundWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SystemIntelligencePlatform.BackgroundWorker.dll"]
