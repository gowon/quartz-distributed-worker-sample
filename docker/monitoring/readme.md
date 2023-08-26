# Grafana

Grafana is the open and composable observability and data visualization platform. Visualize metrics, logs, and traces from multiple sources like Prometheus, Loki, Elasticsearch, InfluxDB, Postgres and many more.

## Containers

|Dependency|Purpose|Image|Path|
|-|-|-|-|
|Grafana Tempo|Distributed Tracing|[`grafana/tempo:latest`](https://hub.docker.com/r/grafana/tempo)||
|Grafana Loki|Distributed Logging|[`grafana/loki:latest`](https://hub.docker.com/r/grafana/loki)||
|Prometheus|Distributed Metrics|[`prom/prometheus:latest`](https://hub.docker.com/r/prom/prometheus)|<http://localhost:9090>|
|Grafana|Monitoring Dashboard|[`grafana/grafana-oss:latest`](https://hub.docker.com/r/grafana/grafana-oss)|<http://localhost:3000>|

## Usage

```powershell
# To spin up all containers run:
docker-compose up -d

# To spin down all containers run:
docker-compose down

# To delete all data run:
docker-compose down -v
```

### Notes

- To know what version of Loki is running, go to `http://localhost:<LOKI_PORT>/loki/api/v1/status/buildinfo`.

## References

- <https://stackoverflow.com/a/71383328/7644876>
