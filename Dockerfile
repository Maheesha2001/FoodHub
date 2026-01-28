# Use .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the project and build
COPY . ./
RUN dotnet publish -c Release -o /app

# Use .NET 9 runtime for final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5187

ENTRYPOINT ["dotnet", "FoodHub.dll"]
