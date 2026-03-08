FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["common.props", "."]
COPY ["src/SystemIntelligencePlatform.HttpApi.Host/SystemIntelligencePlatform.HttpApi.Host.csproj", "src/SystemIntelligencePlatform.HttpApi.Host/"]
COPY ["src/SystemIntelligencePlatform.HttpApi/SystemIntelligencePlatform.HttpApi.csproj", "src/SystemIntelligencePlatform.HttpApi/"]
COPY ["src/SystemIntelligencePlatform.Application/SystemIntelligencePlatform.Application.csproj", "src/SystemIntelligencePlatform.Application/"]
COPY ["src/SystemIntelligencePlatform.Application.Contracts/SystemIntelligencePlatform.Application.Contracts.csproj", "src/SystemIntelligencePlatform.Application.Contracts/"]
COPY ["src/SystemIntelligencePlatform.Domain/SystemIntelligencePlatform.Domain.csproj", "src/SystemIntelligencePlatform.Domain/"]
COPY ["src/SystemIntelligencePlatform.Domain.Shared/SystemIntelligencePlatform.Domain.Shared.csproj", "src/SystemIntelligencePlatform.Domain.Shared/"]
COPY ["src/SystemIntelligencePlatform.EntityFrameworkCore/SystemIntelligencePlatform.EntityFrameworkCore.csproj", "src/SystemIntelligencePlatform.EntityFrameworkCore/"]
COPY ["src/SystemIntelligencePlatform.HttpApi.Client/SystemIntelligencePlatform.HttpApi.Client.csproj", "src/SystemIntelligencePlatform.HttpApi.Client/"]
RUN dotnet restore "src/SystemIntelligencePlatform.HttpApi.Host/SystemIntelligencePlatform.HttpApi.Host.csproj"
COPY src/ ./src/
WORKDIR "/src/src/SystemIntelligencePlatform.HttpApi.Host"
RUN dotnet build "SystemIntelligencePlatform.HttpApi.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemIntelligencePlatform.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SystemIntelligencePlatform.HttpApi.Host.dll"]
