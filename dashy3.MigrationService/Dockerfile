FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore dashy3.MigrationService/dashy3.MigrationService.csproj
RUN dotnet publish dashy3.MigrationService/dashy3.MigrationService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dashy3.MigrationService.dll"]
