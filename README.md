# Lightweight Reverse Proxy / API Gateway

A production-ready reverse proxy and API gateway built with YARP (Yet Another Reverse Proxy) on .NET 9, featuring load balancing, health checks, request logging, and header modification.

## Architecture Overview

```
Client
  |
  v
Reverse Proxy (YARP)
  - Request Logging
  - Header Modification
  - Load Balancing
  - Health Checks
  |
  +--- Backend API Service #1 (Node.js)
  +--- Backend API Service #2 (Node.js)
  +--- Backend API Service #3 (Python)
  +--- Backend API Service #4 (.NET)
```

## Features

- **Load Balancing**: Round-robin distribution across multiple backend services
- **Health Checks**: Automatic detection and removal of unhealthy backends
- **Request Logging**: Comprehensive logging of all requests and responses
- **Header Modification**: Automatic addition of proxy headers (X-Request-ID, X-Forwarded-For, etc.)
- **CORS Support**: Built-in CORS headers for frontend integration
- **Docker Compose**: Easy deployment with Docker
- **Alpine-based Images**: Optimized container sizes (<100MB)
- **Multi-language Backends**: Supports Node.js, Python, and .NET services

## Project Structure

```
Lightweight-Reverse-Proxy-API-Gateway/
├── ReverseProxy/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Middleware/
│   │   ├── RequestLoggingMiddleware.cs
│   │   └── HeaderModificationMiddleware.cs
│   ├── Dockerfile
│   └── ReverseProxy.csproj
├── BackendServices/
│   ├── nodejs-api/
│   │   ├── index.js
│   │   ├── package.json
│   │   └── Dockerfile
│   ├── python-api/
│   │   ├── app.py
│   │   ├── requirements.txt
│   │   └── Dockerfile
│   └── dotnet-api/
│       ├── Program.cs
│       ├── BackendApi.csproj
│       └── Dockerfile
├── docker-compose.yml
└── README.md
```

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- .NET 9 SDK (for local development)
- Node.js 20+ (for local development)
- Python 3.11+ (for local development)

## Quick Start

### 1. Clone the repository

```bash
git clone <repository-url>
cd Lightweight-Reverse-Proxy-API-Gateway
```

### 2. Build and start all services

```bash
docker-compose build
docker-compose up -d
```

### 3. Verify services are running

```bash
docker-compose ps
```

### 4. Test the API

```bash
# Health check
curl http://localhost:8000/api/health

# Get data
curl http://localhost:8000/api/data

# Get service info
curl http://localhost:8000/api/info
```

## API Endpoints

All endpoints are accessible through the reverse proxy at `http://localhost:8000`

### Health Check
```bash
GET /api/health
```
Response:
```json
{
  "status": "healthy",
  "service": "nodejs-api",
  "hostname": "abc123",
  "timestamp": "2026-01-14T13:00:00.000Z"
}
```

### Get Data
```bash
GET /api/data
```
Response:
```json
{
  "message": "Data from Node.js API",
  "service": "nodejs-api",
  "hostname": "abc123",
  "data": {
    "items": ["item1", "item2", "item3"],
    "count": 3
  },
  "timestamp": "2026-01-14T13:00:00.000Z"
}
```

### Get Service Info
```bash
GET /api/info
```
Response:
```json
{
  "service": "nodejs-api",
  "version": "1.0.0",
  "hostname": "abc123",
  "platform": "linux",
  "arch": "x64",
  "nodeVersion": "v20.10.0",
  "uptime": 123.45,
  "timestamp": "2026-01-14T13:00:00.000Z"
}
```

## Testing Load Balancing

Run multiple requests to see load balancing in action:

```bash
# Test with curl (run multiple times)
for i in {1..20}; do
  curl -s http://localhost:8000/api/info | jq '.hostname'
done
```

You should see different hostnames, indicating requests are distributed across backends:
```
"backend-nodejs-1"
"backend-nodejs-2"
"backend-python-1"
"backend-dotnet-1"
"backend-nodejs-1"
...
```

## Load Testing

### Using Apache Bench (ab)

```bash
# Install Apache Bench
sudo apt install apache2-utils

# Run load test: 1000 requests, 10 concurrent
ab -n 1000 -c 10 http://localhost:8000/api/health
```

### Using wrk

```bash
# Install wrk
sudo apt install wrk

# Run load test: 30 seconds, 10 threads, 100 connections
wrk -t10 -c100 -d30s http://localhost:8000/api/health
```

## Viewing Logs

### All services
```bash
docker-compose logs -f
```

### Reverse proxy only
```bash
docker-compose logs -f reverse-proxy
```

### Specific backend
```bash
docker-compose logs -f backend-nodejs-1
```

## Configuration

### Reverse Proxy Configuration (appsettings.json)

```json
{
  "ReverseProxy": {
    "Routes": {
      "api-route": {
        "ClusterId": "backend-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "backend-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Policy": "ConsecutiveFailures",
            "Path": "/api/health"
          }
        },
        "Destinations": {
          "backend-nodejs-1": {
            "Address": "http://backend-nodejs-1:3000"
          }
        }
      }
    }
  }
}
```

### Load Balancing Policies

Available policies:
- `RoundRobin`: Distributes requests evenly in order
- `Random`: Randomly selects a backend
- `LeastRequests`: Sends to backend with fewest active requests
- `PowerOfTwoChoices`: Picks best of two random backends

Change in `appsettings.json`:
```json
"LoadBalancingPolicy": "Random"
```

## Development

### Running Reverse Proxy Locally

```bash
cd ReverseProxy
dotnet restore
dotnet run
```

### Running Backend Services Locally

#### Node.js API
```bash
cd BackendServices/nodejs-api
npm install
npm start
```

#### Python API
```bash
cd BackendServices/python-api
pip install -r requirements.txt
python app.py
```

#### .NET API
```bash
cd BackendServices/dotnet-api
dotnet restore
dotnet run
```

## Docker Commands

### Build all images
```bash
docker-compose build
```

### Start services
```bash
docker-compose up -d
```

### Stop services
```bash
docker-compose down
```

### Restart a specific service
```bash
docker-compose restart reverse-proxy
```

### View container status
```bash
docker-compose ps
```

### Remove everything (including volumes)
```bash
docker-compose down -v
```

### Clean up Docker system
```bash
docker system prune -a
```

## Monitoring

### Check container health
```bash
docker-compose ps
```

### View resource usage
```bash
docker stats
```

### Check logs for errors
```bash
docker-compose logs --tail=100 reverse-proxy | grep -i error
```

## Troubleshooting

### Service won't start

1. Check logs:
```bash
docker-compose logs <service-name>
```

2. Verify port availability:
```bash
sudo netstat -tulpn | grep 8000
```

3. Rebuild containers:
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### Health checks failing

1. Check backend is responding:
```bash
docker exec backend-nodejs-1 wget -O- http://localhost:3000/api/health
```

2. View health check logs:
```bash
docker inspect backend-nodejs-1 | jq '.[0].State.Health'
```

### Load balancing not working

1. Verify all backends are healthy:
```bash
docker-compose ps
```

2. Check YARP logs:
```bash
docker-compose logs reverse-proxy | grep -i health
```

## Performance

Expected performance metrics:
- **Latency**: <5ms proxy overhead
- **Throughput**: 1000+ requests/second
- **Image Size**: <100MB for proxy
- **Memory**: ~50MB per container
- **CPU**: Low CPU usage (<5% idle)

## Security Considerations

- Backend services are not exposed to the internet
- Only reverse proxy port (8000) is accessible externally
- Custom headers track request flow
- Can add authentication middleware
- Can implement rate limiting
- CORS headers are configurable

## Adding New Backends

1. Create new backend service in `BackendServices/`
2. Add service to `docker-compose.yml`
3. Add destination to `appsettings.json`:
```json
"backend-new-service": {
  "Address": "http://backend-new-service:port"
}
```
4. Rebuild and restart:
```bash
docker-compose up -d --build
```

## License

MIT License

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Authors

- Your Name

## Acknowledgments

- YARP (Yet Another Reverse Proxy) by Microsoft
- Docker and Docker Compose
- .NET, Node.js, and Python communities
