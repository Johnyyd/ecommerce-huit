# Deployment Guide - ECommerce HUIT

Tài liệu này hướng dẫn deploy backend API lên các môi trường Staging và Production.

---

## 1. Architecture Overview

```
┌─────────────┐
│   Client    │ (Web/Mobile App)
└──────┬──────┘
       │ HTTPS
┌──────▼──────┐
│   Nginx     │ (reverse proxy, SSL termination)
└──────┬──────┘
       │
┌──────▼─────────────────────────────┐
│   App Server (Ubuntu/Debian)       │
│   - Docker installed               │
│   - docker-compose                 │
│   - Pull images from GHCR          │
└────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────┐
│   SQL Server (Docker or Azure)     │
│   Redis (Docker or Azure Cache)    │
└─────────────────────────────────────┘
```

---

## 2. Pre-deployment Checklist

- [ ] Environment variables configured (`appsettings.Production.json` or Docker secrets)
- [ ] SSL certificate installed (Nginx)
- [ ] Domain DNS points to server IP
- [ ] Firewall allows ports: 80, 443, 22
- [ ] Docker & docker-compose installed
- [ ] Server has enough resources (CPU, RAM, Disk)
- [ ] Database backups scheduled
- [ ] Monitoring (Prometheus/Grafana or Application Insights) configured

---

## 3. Docker Deployment (Recommended)

### 3.1. Build & Push Image to GitHub Container Registry (GHCR)

The CI/CD pipeline already does this on push to `main`. To manually trigger:

```bash
cd ecommerce-huit/BACKEND/src/ECommerce.Huit.API
docker build -t ghcr.io/Johnyyd/ecommerce-huit-api:latest .
docker push ghcr.io/Johnyyd/ecommerce-huit-api:latest
```

You may need to login to GHCR:

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin
```

### 3.2. docker-compose Setup

Create `docker-compose.prod.yml` in the server:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: huit-sqlserver
    environment:
      - SA_PASSWORD=${DB_SA_PASSWORD}
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver-data:/var/opt/mssql
    ports:
      - "1433:1433"
    networks:
      - huit-network
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    container_name: huit-redis
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis-data:/data
    ports:
      - "6379:6379"
    networks:
      - huit-network
    restart: unless-stopped

  api:
    image: ghcr.io/Johnyyd/ecommerce-huit-api:latest
    container_name: huit-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=HuitShopDB;User Id=sa;Password=${DB_SA_PASSWORD};TrustServerCertificate=true
      - ConnectionStrings__Redis=redis:6379,password=${REDIS_PASSWORD}
      - Jwt__Key=${JWT_KEY}
      - Jwt__Issuer=ECommerceHuit
      - Jwt__Audience=ECommerceHuitClient
      - Jwt__DurationInMinutes=1440
    depends_on:
      - sqlserver
      - redis
    ports:
      - "5000:80"
    networks:
      - huit-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3

volumes:
  sqlserver-data:
  redis-data:

networks:
  huit-network:
    driver: bridge
```

**Environment file (`.env`):**

```bash
DB_SA_PASSWORD=YourStrong@Passw0rd
REDIS_PASSWORD=RedisStrongPass123
JWT_KEY=your_64_char_secret_key_here_minimum_32_for_jwt
```

### 3.3. Initialize Database

First run must execute `init.sql` and `seed.sql`:

```bash
# exec into sqlserver container
docker exec -it huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'YourStrong@Passw0rd' \
  -i /path/to/init.sql

docker exec -it huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'YourStrong@Passw0rd' \
  -i /path/to/seed.sql
```

Or you can mount the SQL files in the docker-compose and use an init script.

---

## 4. Nginx Reverse Proxy

Create `/etc/nginx/sites-available/ecommerce-huit`:

```nginx
server {
    listen 80;
    server_name api.huit.edu.vn; # your domain

    # Redirect to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.huit.edu.vn;

    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;
    ssl_protocols TLSv1.2 TLSv1.3;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Health endpoint
    location /health {
        access_log off;
        proxy_pass http://localhost:5000/health;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/ecommerce-huit /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## 5. Zero-Downtime Deploy with Docker Compose

```bash
cd /path/to/ecommerce-huit
docker-compose -f docker-compose.prod.yml pull   # fetch latest image
docker-compose -f docker-compose.prod.yml up -d  # recreate containers
docker-compose -f docker-compose.prod.yml logs -f api  # follow logs
```

Useful commands:

```bash
# Restart API only
docker-compose -f docker-compose.prod.yml restart api

# View logs
docker-compose -f docker-compose.prod.yml logs -f api

# Exec into container
docker-compose -f docker-compose.prod.yml exec api bash

# Stop all
docker-compose -f docker-compose.prod.yml down

# Stop and remove volumes (CAUTION: data loss!)
docker-compose -f docker-compose.prod.yml down -v
```

---

## 6. Database Migrations

If using EF Core migrations (optional, as init.sql already creates schema):

```bash
# From development machine or CI
dotnet ef database update --project BACKEND/src/ECommerce.Huit.API
```

Or apply manually via `sqlcmd` as shown above.

---

## 7. Monitoring & Logs

### Docker Logs

```bash
docker logs huit-api --tail 100 -f
docker logs huit-sqlserver
docker logs huit-redis
```

### Structured Logs (Serilog)

Logs written to `./logs` inside container if mounted:

Add volume to docker-compose:

```yaml
  api:
    ...
    volumes:
      - ./logs:/app/logs
```

Serilog config in `appsettings.Production.json`:

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "/app/logs/log-.txt", "rollingInterval": "Day" }
      }
    ]
  }
}
```

### Health Check

API has built-in health endpoint at `/health`. Configure monitoring to poll it.

---

## 8. Backup Strategy

### SQL Server Backup

```bash
# Backup
docker exec huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'YourStrong@Passw0rd' \
  -Q "BACKUP DATABASE [HuitShopDB] TO DISK = N'/var/opt/mssql/backup/HuitShopDB.bak' WITH INIT"

# Copy backup from container
docker cp huit-sqlserver:/var/opt/mssql/backup/HuitShopDB.bak ./backup/
```

Cron job on host (daily at 2 AM):

```bash
0 2 * * * cd /var/backups/ecommerce && ./backup.sh
```

### Redis Persistence

Redis uses AOF/RDB. Volume `redis-data` persists across restarts.

---

## 9. Scaling

- **Horizontal scaling:** Run multiple API containers behind a load balancer (Nginx upstream or cloud LB)
- **Database:** Move to Azure SQL Database or managed RDS for better scaling and HA
- **Redis:** Use Redis Cluster or Azure Redis Cache
- **CDN:** For static assets (images, docs)

---

## 10. Security Hardening

- Change default `SA` password to something strong
- Disable `sa` login, create dedicated SQL user for app
- Use network isolation: database and Redis should not be exposed to public internet (only accessible from app network)
- Enable firewall rules limiting inbound to ports 22 (SSH), 80, 443
- Keep OS and Docker updated
- Use HTTPS only (let's encrypt certs)
- Store secrets in Docker secrets or environment file with restricted permissions (600)
- Implement rate limiting at Nginx level
- Enable audit logs

---

## 11. Rollback

If new deployment breaks:

```bash
# Find previous image tag (from GitHub tags or local)
docker images | grep ecommerce-huit-api

# Pull previous version
docker pull ghcr.io/Johnyyd/ecommerce-huit-api:v1.2.3

# Update docker-compose image line and restart
docker-compose -f docker-compose.prod.yml up -d
```

Or use `latest` tag with caution; better to pin to specific version.

---

## 12. Troubleshooting

### API not starting

Check logs: `docker logs huit-api`

Common issues:
- Missing environment variable → container exits
- Cannot connect to SQL Server → check network, credentials, DB container status
- Port 5000 already in use → change host port mapping

### Database connection failures

- Ensure SQL Server container is healthy: `docker ps`
- Test connection: `docker exec huit-api dotnet sqlcmd ...` (install sqlcmd in API container)
- Check connection string formatting

### 502 Bad Gateway from Nginx

- API container not running? `docker ps`
- API listening on port 80, not 5000? Check Dockerfile exposes 80, but container runs on 80. Nginx proxies to `localhost:5000` which maps to container's 80 via `ports`. This is correct.

---

## 13. Supporting Advanced Scenarios

### Multi-warehouse

Configure `Warehouse` table and assign `warehouse_id` in order processing. For now, default warehouse 1 is used.

### Webhooks (Payment providers)

Expose public endpoint `/api/payments/webhook`. Configure Nginx to allow unauthenticated POST to that path.

---

**Happy Deploying!** 🚀
