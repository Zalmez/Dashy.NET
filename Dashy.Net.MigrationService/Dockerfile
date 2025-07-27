FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Accept build arguments for version information
ARG GITHUB_REF_NAME
ARG GITHUB_REF_TYPE

COPY Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj ./Dashy.Net.MigrationService/
COPY Dashy.net.Shared/Dashy.Net.Shared.csproj ./Dashy.net.Shared/
COPY Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj ./Dashy.Net.ServiceDefaults/

RUN dotnet restore ./Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj

COPY . .

# Set environment variables for build
ENV GITHUB_REF_NAME=${GITHUB_REF_NAME}
ENV GITHUB_REF_TYPE=${GITHUB_REF_TYPE}

WORKDIR /src/Dashy.Net.MigrationService
RUN dotnet publish Dashy.Net.MigrationService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.MigrationService.dll"]