FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Accept build arguments for version information
ARG GITHUB_REF_NAME
ARG GITHUB_REF_TYPE

# Copy .csproj files needed for the ApiService
COPY ["Dashy.Net.ApiService/Dashy.Net.ApiService.csproj", "./Dashy.Net.ApiService/"]
COPY ["Dashy.net.Shared/Dashy.Net.Shared.csproj", "./Dashy.net.Shared/"]
COPY ["Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj", "./Dashy.Net.ServiceDefaults/"]

# Restore dependencies for the ApiService project only
RUN dotnet restore ./Dashy.Net.ApiService/Dashy.Net.ApiService.csproj

# Copy the rest of the application's source code
COPY . .

# Set environment variables for build
ENV GITHUB_REF_NAME=${GITHUB_REF_NAME}
ENV GITHUB_REF_TYPE=${GITHUB_REF_TYPE}

# Set the workdir to the specific project we want to publish
WORKDIR "/src/Dashy.Net.ApiService"
RUN dotnet publish "Dashy.Net.ApiService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.ApiService.dll"]