#!/bin/bash
set -e

mkdir -p config

# DbMigrator
sed \
  -e "s|{db_host}|${DB_HOST}|g" \
  -e "s|{db_port}|${DB_PORT}|g" \
  -e "s|{db_password}|${DB_PASSWORD}|g" \
  templates/migrator/appsettings.Production.json > config/migrator.appsettings.Production.json

# API
sed \
  -e "s|{db_host}|${DB_HOST}|g" \
  -e "s|{db_port}|${DB_PORT}|g" \
  -e "s|{db_password}|${DB_PASSWORD}|g" \
  -e "s|{redis_host}|${REDIS_HOST}|g" \
  -e "s|{redis_port}|${REDIS_PORT}|g" \
  -e "s|{redis_password}|${REDIS_PASSWORD}|g" \
  templates/api/appsettings.Production.json > config/api.appsettings.Production.json
