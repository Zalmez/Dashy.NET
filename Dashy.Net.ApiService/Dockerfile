FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all .csproj files and the .sln file.
# These paths are relative to the solution root, which we will set as the
# build context in azure.yaml.
COPY ["Dashy.Net.ApiService/Dashy.Net.ApiService.csproj", "Dashy.Net.ApiService/"]
COPY ["Dashy.Net.Web/Dashy.Net.Web.csproj", "Dashy.Net.Web/"]
COPY ["Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj", "Dashy.Net.MigrationService/"]
COPY ["Dashy.net.Shared/Dashy.Net.Shared.csproj", "Dashy.net.Shared/"]
COPY ["Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj", "Dashy.Net.ServiceDefaults/"]
COPY ["Dashy.Net.AppHost/Dashy.Net.AppHost.csproj", "Dashy.Net.AppHost/"]
COPY ["Dashy.Net.sln", "./"]

# Restore dependencies for the entire solution
RUN dotnet restore "Dashy.Net.sln"

# Copy the rest of the application's source code
COPY . .

# Set the workdir to the specific project we want to publish
WORKDIR "/src/Dashy.Net.ApiService"
RUN dotnet publish "Dashy.Net.ApiService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.ApiService.dll"]