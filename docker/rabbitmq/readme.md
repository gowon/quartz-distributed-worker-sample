# RabbitMQ

RabbitMQ is an open source multi-protocol messaging broker.

## Containers

|Container|Purpose|Image|Path|
|-|-|-|-|
|RabbitMQ|Message Queue|[`rabbitmq:3-management-alpine`](https://hub.docker.com/_/rabbitmq)|<http://localhost:15672/>|

## Usage

```powershell
# To spin up all containers run:
docker-compose up -d

# To spin down all containers run:
docker-compose down

# To delete all data run:
docker-compose down -v
```

## References

- <https://www.rabbitmq.com/monitoring.html#health-checks>
- <https://devops.stackexchange.com/a/12200>
