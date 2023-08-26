# PostgreSQL Server

Postgres, is a free and open-source relational database management system (RDBMS) emphasizing extensibility and SQL compliance.

## Containers

|Container|Purpose|Image|Path|
|-|-|-|-|
|PostgreSQL|Relational Database|[`postgres:15-alpine`](https://hub.docker.com/_/postgres)|<http://localhost:5433>|

## Usage

```powershell
# To spin up all containers run:
docker-compose up -d

# To spin down all containers run:
docker-compose down

# To delete all data run:
docker-compose down -v
```

### Initializing Multiple Databases

The shell script `create-additional-postgresql-databases.sh` in the mounted `docker-entrypoint-initdb.d` folder adds support to initialize multiple databases. Using the `POSTGRES_ADDITIONAL_DATABASES` environment variable, you can set multiple databases using a CSV string.

> See <https://github.com/mrts/docker-postgresql-multiple-databases> for more information

## Notes

- By default, if the environment variable `POSTGRES_DB` is not specified, Postgres will create a database with the same name as `POSTGRES_USER`.
- Postgres client `psql` connects to a database named after the user by default. If that does not exist, you will get the error  This is why you get the error: `FATAL:  database "<$POSTGRES_USER>" does not exist`. You can connect to the default system database postgres and then issue your query.

> See <https://stackoverflow.com/a/19426770/7644876>