# Use official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY . .
RUN dotnet restore ./HackerNewsBestStories.Api/HackerNewsBestStories.Api.csproj

# Build and publish the API project
RUN dotnet publish ./HackerNewsBestStories.Api/HackerNewsBestStories.Api.csproj -c Release -o /app/publish

# Use official .NET 8 ASP.NET runtime image for final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose default port
EXPOSE 8080
EXPOSE 8081

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_URLS=https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "HackerNewsBestStories.Api.dll"]