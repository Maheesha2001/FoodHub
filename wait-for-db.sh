#!/bin/sh
set -e

echo "Waiting for MySQL to be ready..."

until mysqladmin ping -h db -P 3306 -u root -proot --silent; do
  echo "MySQL is not ready, waiting 2s..."
  sleep 2
done

echo "MySQL is ready!"
echo "Starting ASP.NET Core..."

exec dotnet FoodHub.dll