# Lightweight Reverse Proxy / API Gateway - MVP on Linux

## MVP Architecture Overview

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────────┐
│  Reverse Proxy (YARP)               │
│  - Request Logging                  │
│  - Header Modification              │
│  - Load Balancing                   │
└──────┬──────────────────────────────┘
       │
       ├─────────────┬─────────────┐
       ▼             ▼             ▼
┌───────────┐  ┌───────────┐  ┌───────────┐
│Backend API│  │Backend API│  │Backend API│
│Service #1 │  │Service #2 │  │Service #3 │
│(Node.js)  │  │(Python)   │  │(.NET)     │
└───────────┘  └───────────┘  └───────────┘
```

---

## Part 1: Project Structure Setup

### What You Need:
1. **Linux Development Environment**
   - Ubuntu/Debian or any Linux distro
   - .NET 8 SDK installed
   - Docker and Docker Compose installed
   - A code editor (VS Code, Rider, or vim)

2. **Project Initialization**
   ```bash
   # Create project directory
   mkdir lightweight-proxy
   cd lightweight-proxy
   
   # Create the proxy project
   dotnet new web -n ReverseProxy
   cd ReverseProxy
   
   # Add YARP package
   dotnet add package Yarp.ReverseProxy
   ```

3. **Folder Structure**
   ```
   lightweight-proxy/
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
   └── docker-compose.yml
   ```

---

## Part 2: Reverse Proxy Implementation

### What You Need:

#### 1. **Program.cs (Main Entry Point)**
   - Configure YARP reverse proxy
   - Register custom middleware
   - Set up routing and load balancing
   - Configure logging

#### 2. **appsettings.json (Configuration)**
   - Define backend clusters
   - Configure load balancing strategy (RoundRobin, Random, etc.)
   - Set up route matching rules
   - Health check endpoints

#### 3. **Custom Middleware**

   **a) RequestLoggingMiddleware.cs**
   - Log incoming request method, path, headers
   - Log response status code and duration
   - Write logs to console and file

   **b) HeaderModificationMiddleware.cs**
   - Add custom headers (X-Forwarded-For, X-Request-ID)
   - Remove sensitive headers
   - Add CORS headers if needed

#### 4. **Key Features to Implement**
   - Request/Response logging
   - Header manipulation
   - Load balancing between backends
   - Basic health checks
   - Error handling

---

## Part 3: Backend Services (For Testing)

### What You Need:

#### 1. **Node.js Backend API**
   - Simple Express server
   - 2-3 endpoints (`/api/health`, `/api/data`, `/api/info`)
   - Returns JSON with hostname/container ID
   - Listens on port 3000

#### 2. **Python Flask Backend API**
   - Simple Flask app
   - Same endpoints as Node.js
   - Returns JSON with hostname
   - Listens on port 5000

#### 3. **.NET Minimal API Backend**
   - ASP.NET Core minimal API
   - Same endpoints
   - Returns JSON with machine name
   - Listens on port 8080

**Purpose:** Each backend returns its identity so you can verify load balancing works.

---

## Part 4: Docker Configuration

### What You Need:

#### 1. **Dockerfile for Reverse Proxy (Alpine-based)**
   - Multi-stage build
   - Stage 1: Build with SDK image
   - Stage 2: Runtime with Alpine image (`mcr.microsoft.com/dotnet/aspnet:8.0-alpine`)
   - Expose port 80
   - Minimal image size (<100MB)

#### 2. **Dockerfiles for Each Backend Service**
   - Node.js: Use `node:alpine`
   - Python: Use `python:alpine`
   - .NET: Use `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`

#### 3. **docker-compose.yml**
   - Define 4 services:
     - `reverse-proxy` (your YARP proxy)
     - `backend-nodejs-1` and `backend-nodejs-2` (two Node.js instances)
     - `backend-python-1` (Python instance)
     - `backend-dotnet-1` (.NET instance)
   - Set up networking
   - Configure ports (only proxy exposed to host)
   - Set up health checks

---

## Part 5: YARP Configuration

### What You Need in appsettings.json:

1. **Routes Section**
   - Define route patterns (`/api/{**catch-all}`)
   - Map routes to backend clusters
   - Set up path transformations if needed

2. **Clusters Section**
   - Define destination groups
   - Configure load balancing policy
   - Set up health check endpoints
   - Define backend URLs

3. **Example Configuration Structure**
   ```json
   {
     "ReverseProxy": {
       "Routes": {
         "route1": {
           "ClusterId": "backend-cluster",
           "Match": {
             "Path": "/api/{**catch-all}"
           }
         }
       },
       "Clusters": {
         "backend-cluster": {
           "LoadBalancingPolicy": "RoundRobin",
           "Destinations": {
             "backend1": { "Address": "http://backend-nodejs-1:3000" },
             "backend2": { "Address": "http://backend-nodejs-2:3000" }
           }
         }
       }
     }
   }
   ```

---

## Part 6: Linux-Specific Implementation

### What You Need:

#### 1. **Docker Setup on Linux**
   ```bash
   # Install Docker
   sudo apt update
   sudo apt install docker.io docker-compose
   
   # Add user to docker group (no sudo needed)
   sudo usermod -aG docker $USER
   # Log out and back in
   ```

#### 2. **Alpine Linux Optimization**
   - Use multi-stage builds
   - Remove unnecessary files
   - Use `--no-cache` in apk commands
   - Configure globalization invariant mode for smaller size

#### 3. **Performance Tuning**
   - Configure Kestrel server limits
   - Set up connection pooling
   - Configure Docker resource limits
   - Use host networking mode for better performance (optional)

#### 4. **Monitoring and Logging**
   - Configure structured logging (JSON)
   - Use Docker logs for output
   - Set up log rotation
   - Monitor container resource usage

---

## Part 7: Testing & Verification

### What You Need:

#### 1. **Testing Tools**
   - `curl` for manual testing
   - `ab` (Apache Bench) or `wrk` for load testing
   - `jq` for JSON parsing

#### 2. **Test Scenarios**
   ```bash
   # Test basic routing
   curl http://localhost:8000/api/health
   
   # Test load balancing (run multiple times)
   for i in {1..10}; do
     curl http://localhost:8000/api/data
   done
   
   # Load testing
   ab -n 1000 -c 10 http://localhost:8000/api/health
   ```

#### 3. **Verification Checklist**
   - All containers running
   - Load balancing distributes requests evenly
   - Custom headers are added
   - Request logging works
   - Health checks pass
   - Response times are acceptable

---

## Part 8: Build & Deployment Steps

### What You Need to Do:

#### 1. **Build Phase**
   ```bash
   # Build all Docker images
   docker-compose build
   
   # Or build individually
   docker build -t reverse-proxy ./ReverseProxy
   docker build -t backend-nodejs ./BackendServices/nodejs-api
   ```

#### 2. **Run Phase**
   ```bash
   # Start all services
   docker-compose up -d
   
   # View logs
   docker-compose logs -f reverse-proxy
   
   # Check container status
   docker-compose ps
   ```

#### 3. **Stop & Cleanup**
   ```bash
   # Stop services
   docker-compose down
   
   # Remove volumes
   docker-compose down -v
   
   # Clean up images
   docker system prune -a
   ```

---

## Complete Checklist for MVP

### Development Environment
- [ ] Linux machine with .NET 8 SDK
- [ ] Docker and Docker Compose installed
- [ ] Code editor set up

### Core Components
- [ ] YARP reverse proxy project created
- [ ] Custom logging middleware implemented
- [ ] Custom header modification middleware implemented
- [ ] appsettings.json configured with routes and clusters
- [ ] Error handling implemented

### Backend Services
- [ ] Node.js API created (2 instances)
- [ ] Python Flask API created
- [ ] .NET minimal API created
- [ ] All backends return identity information

### Docker Setup
- [ ] Dockerfile for proxy (Alpine-based, multi-stage)
- [ ] Dockerfiles for all backend services
- [ ] docker-compose.yml configured
- [ ] Health checks configured

### YARP Features
- [ ] Load balancing configured (RoundRobin)
- [ ] Route matching working
- [ ] Request forwarding working
- [ ] Health checks enabled

### Testing
- [ ] Manual curl tests pass
- [ ] Load balancing verified (requests distributed)
- [ ] Logging works (request/response logged)
- [ ] Headers modified correctly
- [ ] Performance acceptable

### Linux Optimization
- [ ] Alpine base images used
- [ ] Image sizes optimized (<100MB for proxy)
- [ ] Docker resource limits set
- [ ] Structured logging configured

---

## Expected Results

After completing the MVP, you should have:

1. **A working reverse proxy** that can:
   - Forward requests to multiple backends
   - Distribute load evenly (round-robin)
   - Log all requests and responses
   - Add/modify HTTP headers

2. **3-4 backend services** that demonstrate load balancing

3. **Docker deployment** with:
   - Small Alpine-based images
   - All services running in containers
   - Easy start/stop with docker-compose

4. **Performance metrics**:
   - Proxy adds <5ms latency
   - Can handle 1000+ requests/second
   - Image size <100MB

5. **Real-world experience** with:
   - ASP.NET Core middleware
   - YARP configuration
   - Docker on Linux
   - Load balancing concepts

Would you like me to create the actual code for any specific part of this MVP?
