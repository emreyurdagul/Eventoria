# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY Eventoria.Api/Eventoria.Api.csproj Eventoria.Api/
COPY Eventoria.Application/Eventoria.Application.csproj Eventoria.Application/
COPY Eventoria.Domain/Eventoria.Domain.csproj Eventoria.Domain/
COPY Eventoria.Infrastructure/Eventoria.Infrastructure.csproj Eventoria.Infrastructure/

# Restore using the API project
RUN dotnet restore Eventoria.Api/Eventoria.Api.csproj

# Copy everything else
COPY . .

# Publish
RUN dotnet publish Eventoria.Api/Eventoria.Api.csproj -c Release -o /app/publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Eventoria.Api.dll"]
