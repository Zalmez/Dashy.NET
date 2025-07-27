FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG GITHUB_REF_NAME
ARG GITHUB_REF_TYPE

COPY ["Dashy.Net.ApiService/Dashy.Net.ApiService.csproj", "./Dashy.Net.ApiService/"]
COPY ["Dashy.net.Shared/Dashy.Net.Shared.csproj", "./Dashy.net.Shared/"]
COPY ["Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj", "./Dashy.Net.ServiceDefaults/"]

RUN dotnet restore ./Dashy.Net.ApiService/Dashy.Net.ApiService.csproj

COPY . .

ENV GITHUB_REF_NAME=${GITHUB_REF_NAME}
ENV GITHUB_REF_TYPE=${GITHUB_REF_TYPE}

WORKDIR "/src/Dashy.Net.ApiService"
RUN dotnet publish "Dashy.Net.ApiService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.ApiService.dll"]