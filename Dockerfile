# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Node.js for Angular
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs
RUN npm install -g @angular/cli

# Build stage
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy .csproj file from the correct path
COPY ["kizwaonlineshop.Server/kizwaonlineshop.Server.csproj", "kizwaonlineshop.Server/"]
WORKDIR "/src/kizwaonlineshop.Server"

# Restore dependencies
RUN dotnet restore "kizwaonlineshop.Server.csproj"

# Copy the remaining source files
COPY kizwaonlineshop.Server/ kizwaonlineshop.Server/
WORKDIR "/src/kizwaonlineshop.Server"

# Build the project
RUN dotnet build "kizwaonlineshop.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/kizwaonlineshop.Server"
RUN dotnet publish "kizwaonlineshop.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "kizwaonlineshop.Server.dll"]