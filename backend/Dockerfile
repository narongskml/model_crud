# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj and restore
COPY ["PortModelApi/PortModelApi.csproj", "PortModelApi/"]
RUN dotnet restore "PortModelApi/PortModelApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/PortModelApi"
RUN dotnet publish "PortModelApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
COPY --from=build /src/PortModelApi/appsettings*.json .

# Drop to non-root user for security (best practice)
USER app

ENTRYPOINT ["dotnet", "PortModelApi.dll"]
