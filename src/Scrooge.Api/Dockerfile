FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Scrooge.Shared/Scrooge.Shared.csproj Scrooge.Shared/
COPY src/Scrooge.Api/Scrooge.Api.csproj Scrooge.Api/
RUN dotnet restore Scrooge.Api/Scrooge.Api.csproj

COPY src/Scrooge.Shared/ Scrooge.Shared/
COPY src/Scrooge.Api/ Scrooge.Api/
RUN dotnet publish Scrooge.Api/Scrooge.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s CMD curl -f http://localhost:8080/api/setup/status || exit 1
ENTRYPOINT ["dotnet", "Scrooge.Api.dll"]
