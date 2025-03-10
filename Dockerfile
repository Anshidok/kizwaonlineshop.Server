# Use official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["kizwaonlineshop.Server.csproj", "./"]
RUN dotnet restore "./kizwaonlineshop.Server.csproj"
COPY . .
RUN dotnet publish "./kizwaonlineshop.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "kizwaonlineshop.Server.dll"]
