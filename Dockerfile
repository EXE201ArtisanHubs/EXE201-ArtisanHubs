# =========================
#       Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["EXE201_ArtisanHubs.sln", "."]
COPY ["ArtisanHubs.API/ArtisanHubs.API.csproj", "ArtisanHubs.API/"]
COPY ["ArtisanHubs.Bussiness/ArtisanHubs.Bussiness.csproj", "ArtisanHubs.Bussiness/"]
COPY ["ArtisanHubs.Data/ArtisanHubs.Data.csproj", "ArtisanHubs.Data/"]
COPY ["ArtisanHubs.DTOs/ArtisanHubs.DTOs.csproj", "ArtisanHubs.DTOs/"]

# Restore NuGet packages
RUN dotnet restore "ArtisanHubs.sln"

# Copy the remaining source code
COPY . .

# Build and publish API project
WORKDIR "/src/ArtisanHubs.API"
RUN dotnet publish "ArtisanHubs.API.csproj" -c Release -o /app/publish

# =========================
#       Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ArtisanHubs.API.dll"]
