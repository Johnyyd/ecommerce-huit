#!/bin/bash
# Startup script for ECommerce HUIT backend

set -e

echo "=== ECommerce HUIT Backend Setup ==="

# 1. Start Redis (nếu chưa chạy)
if ! docker ps --format '{{.Names}}' | grep -q '^huit-redis$'; then
  echo "Starting Redis..."
  docker run -d --name huit-redis \
    --network ecommerce-network \
    -p 6379:6379 \
    redis:7-alpine \
    redis-server --appendonly yes
else
  echo "Redis already running"
fi

# 2. Start SQL Server (nếu chưa chạy)
if ! docker ps --format '{{.Names}}' | grep -q '^huit-sqlserver$'; then
  echo "Starting SQL Server..."
  docker run -d --name huit-sqlserver \
    --network ecommerce-network \
    -e 'ACCEPT_EULA=Y' \
    -e 'SA_PASSWORD=YourStrong@Passw0rd' \
    -p 1433:1433 \
    -v $(pwd)/DATABASE:/var/opt/mssql/backup \
    mcr.microsoft.com/mssql/server:2022-latest
else
  echo "SQL Server already running"
fi

# 3. Wait for SQL Server to accept connections
echo "Waiting for SQL Server to accept connections on port 1433..."
attempt=0
max_attempts=60
while ! docker exec huit-sqlserver /bin/bash -c "timeout 1 bash -c '</dev/tcp/localhost/1433'" 2>/dev/null; do
  attempt=$((attempt + 1))
  if [ $attempt -ge $max_attempts ]; then
    echo "SQL Server did not become ready in time"
    exit 1
  fi
  echo -n "."
  sleep 2
done
echo ""
echo "SQL Server is accepting connections"

# 4. Initialize database (only if not already initialized)
echo "Checking if database exists..."
DB_EXISTS=$(docker exec huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P 'YourStrong@Passw0rd' \
  -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'HuitShopDB') SELECT 1 ELSE SELECT 0" \
  2>/dev/null | tail -n 1 | tr -d ' ' || echo 0)

if [ "$DB_EXISTS" = "0" ]; then
  echo "Initializing database..."
  docker exec huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U SA -P 'YourStrong@Passw0rd' \
    -i /var/opt/mssql/backup/init.sql
  docker exec huit-sqlserver /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U SA -P 'YourStrong@Passw0rd' \
    -i /var/opt/mssql/backup/seed.sql
  echo "Database initialized"
else
  echo "Database already exists"
fi

# 5. Build and run backend API
echo "Building backend API image..."
cd BACKEND/src
docker build -t ecommerce-huit-api .

# Start API (nếu chưa chạy)
if ! docker ps --format '{{.Names}}' | grep -q '^huit-api$'; then
  echo "Running backend API..."
  cd ../..
  docker run -d --name huit-api \
    --network ecommerce-network \
    -p 5000:5000 \
    -v "$(pwd)/DATABASE:/app/DATABASE:ro" \
    -e "ConnectionStrings__DefaultConnection=Server=huit-sqlserver;Database=HuitShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true" \
    -e "Jwt__Key=your_64_char_secret_key_here_minimum_32_chars_for_jwt_123456" \
    ecommerce-huit-api
else
  echo "Backend API already running"
  cd ../..
fi


bash start-backend.sh


echo ""
echo "=== All services started! ==="
echo "Frontend: http://localhost:5173"
echo "Backend API: http://localhost:5000"
echo "Swagger: http://localhost:5000/swagger"
echo ""
echo "To view logs:"
echo "  docker logs -f huit-api"
echo "  docker logs -f huit-sqlserver"
echo ""
echo "To stop:"
echo "  docker stop huit-api huit-sqlserver huit-redis"
echo "  docker network rm ecommerce-network"
