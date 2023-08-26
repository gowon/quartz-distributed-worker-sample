#!/bin/bash

set -e
set -u

function create_user_and_database() {
	local database=$1
	echo "  Creating user and database '$database'"
	psql -v ON_ERROR_STOP=1 -d postgres --username "$POSTGRES_USER" <<-EOSQL
	    CREATE USER $database;
	    CREATE DATABASE $database;
	    GRANT ALL PRIVILEGES ON DATABASE $database TO $database;
	    GRANT ALL PRIVILEGES ON DATABASE $database TO $POSTGRES_USER;
EOSQL
}

if [ -n "$POSTGRES_ADDITIONAL_DATABASES" ]; then
	echo "Additional database creation requested: $POSTGRES_ADDITIONAL_DATABASES"
	for db in $(echo $POSTGRES_ADDITIONAL_DATABASES | tr ',' ' '); do
		if [[ "$db" == "$POSTGRES_DB" ]]; then
			continue
		fi

		create_user_and_database $db
	done
	echo "Additional databases created"
fi