FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Accept build arguments for version information
ARG GITHUB_REF_NAME
ARG GITHUB_REF_TYPE

COPY Dashy.Net.Web/Dashy.Net.Web.csproj ./Dashy.Net.Web/
COPY Dashy.Net.ApiService/Dashy.Net.ApiService.csproj ./Dashy.Net.ApiService/
COPY Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj ./Dashy.Net.MigrationService/
COPY Dashy.net.Shared/Dashy.Net.Shared.csproj ./Dashy.net.Shared/
COPY Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj ./Dashy.Net.ServiceDefaults/

RUN dotnet restore ./Dashy.Net.Web/Dashy.Net.Web.csproj

COPY . .

WORKDIR /src/Dashy.Net.Web

# Set environment variables for build
ENV GITHUB_REF_NAME=${GITHUB_REF_NAME}
ENV GITHUB_REF_TYPE=${GITHUB_REF_TYPE}

RUN dotnet publish Dashy.Net.Web.csproj -c Release -o /app/publish /p:UseAppHost=falseoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Dashy.Net.Web/Dashy.Net.Web.csproj ./Dashy.Net.Web/
COPY Dashy.Net.ApiService/Dashy.Net.ApiService.csproj ./Dashy.Net.ApiService/
COPY Dashy.Net.MigrationService/Dashy.Net.MigrationService.csproj ./Dashy.Net.MigrationService/
COPY Dashy.net.Shared/Dashy.Net.Shared.csproj ./Dashy.net.Shared/
COPY Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj ./Dashy.Net.ServiceDefaults/

RUN dotnet restore ./Dashy.Net.Web/Dashy.Net.Web.csproj

COPY . .

WORKDIR /src/Dashy.Net.Web
RUN dotnet publish Dashy.Net.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.Web.dll"]