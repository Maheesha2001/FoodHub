# 1. Use .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# 2. Copy project file and restore dependencies
COPY FoodHub.csproj .
RUN dotnet restore

# 3. Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 4. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# 5. Copy published app from build stage
COPY --from=build /app/publish .

# 6. Expose ports
EXPOSE 5000
EXPOSE 5001

# 7. Run the app
ENTRYPOINT ["dotnet", "FoodHub.dll"]
