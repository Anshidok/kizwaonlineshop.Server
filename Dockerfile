# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image with Node.js
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update && apt-get install -y curl && \
    curl -sL https://deb.nodesource.com/setup_20.x | bash && \
    apt-get -y install nodejs && \
    npm install -g @angular/cli

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["kizwaonlineshop.Server.csproj", "./"]
RUN dotnet restore "./kizwaonlineshop.Server.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "./kizwaonlineshop.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "./kizwaonlineshop.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "kizwaonlineshop.Server.dll"]
