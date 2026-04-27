FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# dashy3.Web runs npm and Tailwind during build, so Node.js is required in the build stage.
RUN apt-get update \
    && apt-get install -y curl gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

COPY . .
RUN dotnet restore dashy3.Web/dashy3.Web.csproj
RUN dotnet publish dashy3.Web/dashy3.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dashy3.Web.dll"]
