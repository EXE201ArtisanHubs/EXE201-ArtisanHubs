# Build stage
FROM mcr.microsoft.com / dotnet / sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY *.sln .
COPY ArtisanHubs.API/*.csproj ./ArtisanHubs.API/
COPY ArtisanHubs.Bussiness/*.csproj ./ArtisanHubs.Bussiness/
COPY ArtisanHubs.Data/*.csproj ./ArtisanHubs.Data/
COPY ArtisanHubs.DTOs/*.csproj ./ArtisanHubs.DTOs/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src/ArtisanHubs.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ArtisanHubs.API.dll"]