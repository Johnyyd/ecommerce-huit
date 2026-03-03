# Triển khai (Deployment) - ECommerce HUIT

Hướng dẫn triển khai ứng dụng trong môi trường production.

---

## 📦 Mục lục

1. [Yêu cầu Hạ tầng](#yêu-cầu-hạ-tầng)
2. [Environment Variables](#environment-variables)
3. [Database Setup](#database-setup)
4. [Docker Deployment](#docker-deployment)
5. [Kubernetes (K8s) Deployment](#kubernetes-k8s-deployment)
6. [CI/CD với GitHub Actions](#cicd-với-github-actions)
7. [SSL/TLS](#ssltls)
8. [Monitoring](#monitoring)
9. [Backup & Recovery](#backup--recovery)
10. [Scaling](#scaling)

---

## Yêu cầu Hạ tầng

### Minimum Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **Server** | 2 vCPU, 4GB RAM | 4 vCPU, 8GB RAM |
| **OS** | Ubuntu 22.04 LTS | Ubuntu 22.04 LTS |
| **Docker** | 20.10+ | 24.0+ |
| **SQL Server** | 2019+ Express | 2022+ Standard |
| **Redis** | 6.x | 7.x |
| **Domain** | Optional | Required |

### Network

- Port 80 (HTTP) → redirect to 443
- Port 443 (HTTPS)
- Port 5000 (Docker internal)
- Database: 1433 (internal only, not public)

---

## Environment Variables

### Backend (`ECommerce.Huit.API`)

```bash
# Required
ConnectionStrings__DefaultConnection="Server=db;Database=HuitShopDB;User Id=sa;Password=..."
Jwt__Key="your-64-characters-super-secret-key-minimum"
Jwt__Issuer="ECommerceHuit"
Jwt__Audience="ECommerceHuitClient"
Jwt__DurationInMinutes="1440"

# Optional
Redis__ConnectionString="redis:6379"
Serilog__WriteTo__0__Args__path="/logs/log-.txt"
Email__SmtpHost="smtp.gmail.com"
Email__SmtpPort="587"
Email__Username="your@gmail.com"
Email__Password="app_password"
```

### Docker Compose

Tạo file `.env` trong thư mục root:

```bash
# SQL Server
SA_PASSWORD=YourStrong@Passw0rd
ACCEPT_EULA=Y
MSSQL_PID=Express
MSSQL_DATA_DIR=/var/opt/mssql/data

# Redis
REDIS_PASSWORD=OptionalRedisPass

# JWT (should be in secret manager in prod)
JWT_SECRET=your-production-secret-64-chars-long-change-this

# Email
SMTP_USERNAME=
SMTP_PASSWORD=
```

---

## Database Setup

### 1. Initialize Database

```bash
# Using sqlcmd (install mssql-tools)
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i DATABASE/init.sql
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i DATABASE/seed.sql
```

### 2. Create Backup User (optional)

```sql
CREATE LOGIN backup_user WITH PASSWORD = 'StrongBackupPass123';
CREATE USER backup_user FOR LOGIN backup_user;
EXEC sp_addrolemember 'db_backupoperator', 'backup_user';
GRANT SELECT, INSERT, UPDATE, DELETE ON DATABASE::HuitShopDB TO backup_user;
```

### 3. Enable Query Store (SQL Server 2019+)

```sql
ALTER DATABASE HuitShopDB SET QUERY_STORE = ON;
ALTER DATABASE HuitShopDB SET QUERY_STORE (OPERATION_MODE = READ_WRITE);
```

### 4. Configure Max Degree of Parallelism (MAXDOP)

```sql
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'max degree of parallelism', 1;
RECONFIGURE;
```

---

## Docker Deployment

### 1. Build Images

```bash
cd /path/to/ecommerce-huit
docker-compose build
```

### 2. Production Environment File

Tạo `.env.prod`:

```bash
# Database
MSSQL_SA_PASSWORD=SuperStrongProdPass123!@#
MSSQL_DATABASE=HuitShopDB_Prod
MSSQL_PID=Standard

# JWT
JWT_SECRET=prod-super-secret-key-with-64-characters-minimum-change-this-now

# Redis
REDIS_PASSWORD=prod-redis-pass

# App
ASPNETCORE_ENVIRONMENT=Production
DOTNET_VERSION=8.0
```

### 3. Run with Docker Compose

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### 4. Check Logs

```bash
docker-compose logs -f backend
docker-compose logs -f sqlserver
docker-compose logs -f redis
```

### 5. Database Migrations

Nếu dùng EF Core migrations (chưa setup), chạy:

```bash
docker-compose exec backend dotnet ef database update
```

### 6. Initialize Data (Optional)

```bash
# Run seed script if needed
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'SuperStrongProdPass123!' \
  -d HuitShopDB_Prod \
  -i /scripts/seed.sql
```

---

## Kubernetes (K8s) Deployment

### 1. Create Namespace

```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: ecommerce-huit
```

### 2. Secrets

```yaml
# k8s/secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: ecommerce-secrets
  namespace: ecommerce-huit
type: Opaque
stringData:
  connection-string: "Server=sqlserver;Database=HuitShopDB;User Id=sa;Password=${SQL_SA_PASSWORD};"
  jwt-key: "${JWT_SECRET}"
  redis-connection: "redis:6379,password=${REDIS_PASSWORD}"
```

### 3. ConfigMap

```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: ecommerce-config
  namespace: ecommerce-huit
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Jwt__Issuer: "ECommerceHuit"
  Jwt__Audience: "ECommerceHuitClient"
  Jwt__DurationInMinutes: "1440"
```

### 4. Deployment

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ecommerce-api
  namespace: ecommerce-huit
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ecommerce-api
  template:
    metadata:
      labels:
        app: ecommerce-api
    spec:
      containers:
      - name: api
        image: ghcr.io/johnyyd/ecommerce-huit:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: ecommerce-config
        - secretRef:
            name: ecommerce-secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

### 5. Service

```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: ecommerce-api-service
  namespace: ecommerce-huit
spec:
  selector:
    app: ecommerce-api
  ports:
  - port: 80
    targetPort: 80
    protocol: TCP
  type: LoadBalancer
```

### 6. Ingress (with TLS)

```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ecommerce-ingress
  namespace: ecommerce-huit
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - api.huit.com
    secretName: ecommerce-tls
  rules:
  - host: api.huit.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: ecommerce-api-service
            port:
              number: 80
```

### 7. Apply

```bash
kubectl apply -f k8s/
```

---

## CI/CD với GitHub Actions

Workflows đã được tạo trong `.github/workflows/`:

### CI Workflow (`ci.yml`)

- **Trigger:** Push/PR vào `main` hoặc `develop`
- **Services:** SQL Server + Redis
- **Steps:**
  1. Checkout
  2. Setup .NET 8
  3. Restore dependencies
  4. Build
  5. Create test database
  6. Run tests (Unit + Integration)
  7. Upload test results (TRX)
  8. Run `dotnet format` for linting

### CD Workflow (`cd.yml`)

- **Trigger:** Push vào `main`
- **Steps:**
  1. Build Docker image với multi-stage
  2. Tag với `git sha` và `latest`
  3. Push lên `ghcr.io` (GitHub Container Registry)
  4. (Optional) Deploy đến staging server qua SSH

**Cấu hình Secrets trên GitHub:**

- `DOCKERHUB_USERNAME` (nếu dùng Docker Hub)
- `DOCKERHUB_TOKEN`
- `STAGING_SSH_KEY` (private key để SSH)
- `STAGING_HOST`, `STAGING_USER`

---

## SSL/TLS

### Backend (Kestrel)

Trong production, nên để Kestrel chạy behind reverse proxy (NGINX/Apache). Nếu cần HTTPS trực tiếp:

```bash
# Tạo certificate (self-signed for testing)
dotnet dev-certs https -ep /path/to/cert.pfx -p password

# Trong Program.cs:
.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps("/path/to/cert.pfx", "password");
    });
});
```

### Nginx Reverse Proxy

```nginx
server {
    listen 80;
    server_name api.huit.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.huit.com;

    ssl_certificate /etc/letsencrypt/live/api.huit.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.huit.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Monitoring

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog + ELK

Thêm sink trong `appsettings.json`:

```json
"Serilog": {
  "WriteTo": [
    { "Name": "Elasticsearch", "Args": { "nodeUris": "http://localhost:9200", "indexFormat": "logstash-{0:yyyy.MM.dd}" } }
  ]
}
```

### Prometheus + Grafana (Metrics)

```csharp
// Install-Package Prometheus.AspNetCore
app.UseHttpMetrics();
```

Metrics endpoint: `/metrics`

---

## Backup & Recovery

### SQL Server Backup

```bash
# Full backup
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -Q "BACKUP DATABASE [HuitShopDB] TO DISK = N'/var/opt/mssql/backup/HuitShopDB_full.bak' WITH FORMAT, INIT, NAME = 'HuitShopDB-Full'"

# Diff backup
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -Q "BACKUP DATABASE [HuitShopDB] TO DISK = N'/var/opt/mssql/backup/HuitShopDB_diff.bak' WITH DIFFERENTIAL"

# Restore
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -Q "RESTORE DATABASE [HuitShopDB] FROM DISK = N'/var/opt/mssql/backup/HuitShopDB_full.bak' WITH REPLACE"
```

**Cron job Backup hàng ngày:**

```bash
# /etc/cron.d/sqlbackup
0 2 * * * root /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -Q "BACKUP DATABASE [HuitShopDB] TO DISK = N'/backup/HuitShopDB_$(date +\%Y\%m\%d).bak'"
```

### Redis Backup

```bash
# Redis RDB snapshot
docker exec redis redis-cli SAVE
docker cp redis:/data/dump.rdb /backup/redis-$(date +%Y%m%d).rdb
```

---

## Scaling

### Horizontal Scaling (API)

- Deploy nhiều instance của backend API
- Load balancing với Nginx/HAProxy
- Session state dùng Redis (distributed cache)

### Database Scaling

- Read replicas (SQL Server Always On)
- Sharding theo tenant/region (future)

### Redis Scaling

- Redis Cluster (sharding)
- Redis Sentinel (HA)

---

## Security Checklist

- [x] Enable HTTPS only (no HTTP)
- [x] Strong JWT secret (64+ chars)
- [x] Short-lived access tokens (24h)
- [x] Refresh tokens stored in DB (not JWT)
- [x] Rate limiting middleware
- [x] CORS restricted to known domains
- [x] SQL Injection protected (EF Core + parameterized queries)
- [x] XSS protection headers
- [x] CSRF tokens (if needed for state-changing operations)
- [x] Secret management (Azure Key Vault, AWS Secrets, HashiCorp Vault)
- [x] Database firewall (allow only app server)
- [x] Regular backups (daily + weekly)
- [x] Audit logs (already in schema)

---

## Maintenance

### Health Checks

- `GET /health` - Liveness probe (DB, Redis connectivity)
- `GET /ready` - Readiness probe (app initialized)

### Log Rotation

Giám sát file `logs/log-.txt`, dùng logrotate:

```
/var/log/ecommerce/*.log {
    daily
    rotate 30
    compress
    missingok
    notifempty
    create 644 www-data www-data
    postrotate
        systemctl reload nginx || true
    endscript
}
```

---

## Troubleshooting

### Database Connection Issues

```bash
# Test connectivity from app container
docker-compose exec backend dotnet ef dbcontext info
docker-compose exec backend ping sqlserver
```

### View SQL Server Error Logs

```bash
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'password' \
  -Q "EXEC sp_readerrorlog 0, 1, N'Login failed'"
```

### Redis CLI

```bash
docker-compose exec redis redis-cli
> ping
> info
> monitor
```

---

## Support

Liên hệ dev team nếu gặp vấn đề triển khai.

---

**Happy Deploying! 🚀**
