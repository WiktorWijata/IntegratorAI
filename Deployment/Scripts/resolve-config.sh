#!/bin/bash
set -e

mkdir -p config

# DbMigrator
export DB_HOST DB_PORT DB_PASSWORD
envsubst '${DB_HOST}${DB_PORT}${DB_PASSWORD}' \
  < templates/migrator/appsettings.Production.json \
  > config/migrator.appsettings.Production.json

# API
export DB_HOST DB_PORT DB_PASSWORD REDIS_HOST REDIS_PORT REDIS_PASSWORD
envsubst '${DB_HOST}${DB_PORT}${DB_PASSWORD}${REDIS_HOST}${REDIS_PORT}${REDIS_PASSWORD}' \
  < templates/api/appsettings.Production.json \
  > config/api.appsettings.Production.json
