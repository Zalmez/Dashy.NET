FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj ./Dashy.Net.MigrationService/
COPY Dashy.Net.ApiService/Dashy.Net.ApiService.csproj ./Dashy.Net.ApiService/
COPY Dashy.Net.Web/Dashy.Net.Web.csproj ./Dashy.Net.Web/
COPY Dashy.net.Shared/Dashy.Net.Shared.csproj ./Dashy.Net.Shared/
COPY Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj ./Dashy.Net.ServiceDefaults/

RUN dotnet restore ./Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj

COPY . .

WORKDIR /src/Dashy.Net.MigrationService
RUN dotnet publish Dashy.Net.MigrationService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.MigrationService.dll"]