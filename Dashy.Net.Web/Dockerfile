FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG GITHUB_REF_NAME
ARG GITHUB_REF_TYPE

COPY ["Dashy.Net.Web/Dashy.Net.Web.csproj", "./Dashy.Net.Web/"]
COPY ["Dashy.net.Shared/Dashy.Net.Shared.csproj", "./Dashy.net.Shared/"]
COPY ["Dashy.Net.ServiceDefaults/Dashy.Net.ServiceDefaults.csproj", "./Dashy.Net.ServiceDefaults/"]

RUN dotnet restore ./Dashy.Net.Web/Dashy.Net.Web.csproj

COPY . .

ENV GITHUB_REF_NAME=${GITHUB_REF_NAME}
ENV GITHUB_REF_TYPE=${GITHUB_REF_TYPE}

WORKDIR "/src/Dashy.Net.Web"
RUN dotnet publish "Dashy.Net.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Dashy.Net.Web.dll"]