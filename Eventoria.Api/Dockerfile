# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Eventoria.Api/Eventoria.Api.csproj Eventoria.Api/
COPY Eventoria.Application/Eventoria.Application.csproj Eventoria.Application/
COPY Eventoria.Domain/Eventoria.Domain.csproj Eventoria.Domain/
COPY Eventoria.Infrastructure/Eventoria.Infrastructure.csproj Eventoria.Infrastructure/

RUN dotnet restore Eventoria.Api/Eventoria.Api.csproj
COPY . .

RUN dotnet publish Eventoria.Api/Eventoria.Api.csproj -c Release -o /app/publish

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# ✅ FIX: install libgssapi_krb5.so.2 provider
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Eventoria.Api.dll"]
