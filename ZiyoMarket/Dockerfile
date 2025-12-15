# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ZiyoMarket.sln ./

# Copy project files
COPY src/ZiyoMarket.Api/ZiyoMarket.Api.csproj src/ZiyoMarket.Api/
COPY src/ZiyoMarket.Domain/ZiyoMarket.Domain.csproj src/ZiyoMarket.Domain/
COPY src/ZiyoMarket.Service/ZiyoMarket.Service.csproj src/ZiyoMarket.Service/
COPY src/ZiyoMarket.Data/ZiyoMarket.Data.csproj src/ZiyoMarket.Data/

# Restore dependencies only for API project (excludes Windows-only AdminPanel)
RUN dotnet restore src/ZiyoMarket.Api/ZiyoMarket.Api.csproj

# Copy source code
COPY . .

# Build the application
WORKDIR /src/src/ZiyoMarket.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Expose port (Railway will assign port dynamically)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "ZiyoMarket.Api.dll"]
