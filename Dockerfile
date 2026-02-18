# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FoodHub.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish FoodHub.csproj -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN apt-get update && \
    apt-get install -y default-mysql-client && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

COPY wait-for-db.sh .
RUN chmod +x wait-for-db.sh

EXPOSE 5187
ENV ASPNETCORE_URLS=http://0.0.0.0:5187

ENTRYPOINT ["./wait-for-db.sh"]