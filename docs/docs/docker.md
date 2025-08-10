---
sidebar_position: 4
---

# Docker Deployment

Docker is the recommended way to deploy Dashy.NET in production. This guide covers everything you need to know about running Dashy.NET with Docker.

## Quick Start with Docker Compose

The fastest way to get Dashy.NET running is with Docker Compose:

### 1. Download Required Files

```bash
# Create a directory for Dashy.NET
mkdir dashy-net && cd dashy-net

# Download the compose file and environment example
curl -o docker-compose.yml https://raw.githubusercontent.com/Zalmez/Dashy.NET/main/docker-compose.yml
curl -o .env.example https://raw.githubusercontent.com/Zalmez/Dashy.NET/main/.env.example

# Copy and customize the environment file
cp .env.example .env
```

### 2. Configure Environment (Optional)

Edit the `.env` file to customize settings:

```bash
# Edit with your preferred editor
nano .env
```

**Important**: Change the `DB_PASSWORD` from the default value for security!

### 3. Start the Services

```bash
docker-compose up -d
```

### 4. Access Your Dashboard

Open your browser and navigate to `http://localhost:8080`

## Docker Compose Configuration

Here's the complete `docker-compose.yml` for reference:

```yaml
services:
  db:
    image: postgres:16
    restart: unless-stopped
    env_file: .env
    ports:
      - "5432:5432"
    volumes:
      - db_data:/var/lib/postgresql/data

  migrationservice:
    image: ghcr.io/zalmez/dashy.net/migrationservice:latest
    restart: unless-stopped
    env_file: .env
    depends_on:
      - db

  apiservice:
    image: ghcr.io/zalmez/dashy.net/apiservice:latest
    restart: unless-stopped
    env_file: .env
    depends_on: 
      - migrationservice
    ports:
      - "8080:80"
      - "8443:443"

  webfrontend:
    image: ghcr.io/zalmez/dashy.net/webfrontend:latest
    restart: unless-stopped
    env_file: .env
    depends_on:
      - apiservice
    ports:
      - "5000:80"
      - "5443:443"

volumes:
  db_data:

```

## Environment Variables

Customize your deployment with these environment variables:

### Database Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `DB_PASSWORD` | PostgreSQL password | `changeme123` |
| `POSTGRES_DB` | Database name | `dashynet` |
| `POSTGRES_USER` | Database username | `dashynet` |
| `DB_PORT` | Database port | `5432` |

### Application Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | App environment | `Development` |
| `ConnectionStrings__dashy` | Database connection string for .NET apps | `Host=db;Database=dashynet;Username=dashynet;Password=changeme123` |

### Creating Environment File

Copy the example environment file and customize it:

```bash
# Copy the example file
cp .env.example .env

# Edit the file with your preferred values
nano .env  # or use your preferred editor
```

The `.env.example` file contains all available configuration options:

```bash
# Database Configuration
DB_PASSWORD=changeme123

# Optional: Custom ports
# WEB_PORT=8080
# DB_PORT=5432

# Application Environment
ASPNETCORE_ENVIRONMENT=Production
```

Then reference it in your compose command:
```bash
docker-compose --env-file .env up -d
```

## Manual Docker Commands

If you prefer to run containers manually:

### 1. Create a Network

```bash
docker network create dashy-network
```

### 2. Start PostgreSQL

```bash
docker run -d \
  --name dashy-postgres \
  --network dashy-network \
  -e POSTGRES_DB=DashyNet \
  -e POSTGRES_USER=dashynet \
  -e POSTGRES_PASSWORD=your-secure-password \
  -v dashy-postgres-data:/var/lib/postgresql/data \
  -p 5432:5432 \
  --restart unless-stopped \
  postgres:15
```

### 3. Start Dashy.NET

```bash
docker run -d \
  --name dashy-net \
  --network dashy-network \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=dashy-postgres;Database=DashyNet;Username=dashynet;Password=your-secure-password" \
  -p 8080:80 \
  --restart unless-stopped \
  zalmez/dashy-net:latest
```

## Persistent Data

### Database Persistence

Your dashboard configuration is stored in PostgreSQL. Ensure data persistence by:

1. **Using Docker volumes** (recommended):
   ```yaml
   volumes:
     - postgres_data:/var/lib/postgresql/data
   ```

2. **Using bind mounts**:
   ```yaml
   volumes:
     - ./data/postgres:/var/lib/postgresql/data
   ```

### Backup and Restore

#### Backup Database

```bash
# Create backup
docker exec dashy-postgres pg_dump -U dashynet DashyNet > backup.sql

# Or with Docker Compose
docker-compose exec postgres pg_dump -U dashynet DashyNet > backup.sql
```

#### Restore Database

```bash
# Restore backup
docker exec -i dashy-postgres psql -U dashynet DashyNet < backup.sql

# Or with Docker Compose
docker-compose exec -T postgres psql -U dashynet DashyNet < backup.sql
```

## Reverse Proxy Configuration

### Nginx

```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Traefik

```yaml
services:
  web:
    image: zalmez/dashy-net:latest
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.dashy.rule=Host(`your-domain.com`)"
      - "traefik.http.routers.dashy.entrypoints=websecure"
      - "traefik.http.routers.dashy.tls.certresolver=myresolver"
      - "traefik.http.services.dashy.loadbalancer.server.port=80"
```

### Caddy

```
your-domain.com {
    reverse_proxy localhost:8080
}
```

## SSL/HTTPS Configuration

### Option 1: Reverse Proxy SSL

Use a reverse proxy (Nginx, Traefik, Caddy) to handle SSL termination.

### Option 2: Container SSL

```yaml
services:
  web:
    image: zalmez/dashy-net:latest
    environment:
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Password=cert-password
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/certificate.pfx
    volumes:
      - ./certs:/https
    ports:
      - "8443:443"
      - "8080:80"
```

## Health Checks

Add health checks to your Docker Compose:

```yaml
services:
  web:
    image: zalmez/dashy-net:latest
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/api/version"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
```

## Monitoring and Logs

### View Logs

```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View logs for specific service
docker-compose logs web
docker-compose logs postgres
```

### Log Configuration

Configure logging levels via environment variables:

```yaml
environment:
  - Logging__LogLevel__Default=Information
  - Logging__LogLevel__Microsoft.AspNetCore=Warning
```

## Updates

### Updating Dashy.NET

```bash
# Pull latest images
docker-compose pull

# Restart with new images
docker-compose up -d

# Remove old images
docker image prune
```

### Automated Updates

Use [Watchtower](https://github.com/containrrr/watchtower) for automatic updates:

```yaml
services:
  watchtower:
    image: containrrr/watchtower
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    environment:
      - WATCHTOWER_POLL_INTERVAL=86400  # Check daily
      - WATCHTOWER_CLEANUP=true
```

## Troubleshooting

### Common Issues

**Container won't start**:
```bash
# Check logs
docker-compose logs web

# Check if ports are available
netstat -tulpn | grep :8080
```

**Database connection issues**:
```bash
# Test database connection
docker-compose exec web curl postgres:5432

# Check database logs
docker-compose logs postgres
```

**Permission issues**:
```bash
# Ensure proper ownership of volumes
sudo chown -R 999:999 ./data/postgres  # PostgreSQL UID
```

### Performance Tuning

**PostgreSQL optimization**:
```yaml
services:
  postgres:
    image: postgres:15
    environment:
      - POSTGRES_INITDB_ARGS=--auth-host=scram-sha-256
    command: >
      postgres
      -c shared_preload_libraries=pg_stat_statements
      -c max_connections=200
      -c shared_buffers=256MB
      -c effective_cache_size=1GB
```

**Application optimization**:
```yaml
services:
  web:
    image: zalmez/dashy-net:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - COMPlus_ThreadPool_ForceMinWorkerThreads=50
    deploy:
      resources:
        limits:
          memory: 512M
        reservations:
          memory: 256M
```

## Security Considerations

1. **Change default passwords**: Always set strong passwords
2. **Use secrets**: Store sensitive data in Docker secrets
3. **Network isolation**: Use custom Docker networks
4. **Regular updates**: Keep containers updated
5. **Backup encryption**: Encrypt database backups
6. **Firewall rules**: Restrict network access appropriately

## Next Steps

- [Configure your dashboard](/docs/configuration)
- [Set up authentication](/docs/authentication)
- [Learn about widgets](/docs/widgets)
