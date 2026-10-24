
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file from the repository root context.
COPY ["Sanay3y.API/Sanay3y.API.csproj", "Sanay3y.API/"]

RUN dotnet restore "Sanay3y.API/Sanay3y.API.csproj"

# Copy project source files.
COPY ["Sanay3y.API/", "Sanay3y.API/"]

WORKDIR /src/Sanay3y.API

RUN dotnet publish "Sanay3y.API.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sanay3y.API.dll"]
