# High-Throughput URL Shortener

A production-grade URL Shortener engine built with **ASP.NET Core Minimal APIs**, optimized using **Redis Distributed Caching** for sub-millisecond lookups, and backed by **SQLite** for relational tracking.

## Architecture Highlights
- **Base-62 ID Mapping**: Uses unique generated database increments translated via Hashids.
- **Write-Through Cache Layer**: Pre-primes the Redis cache instantly upon key allocation.
- **Read Cache Optimization**: Bypass persistent database reads completely via key-value caching filters.

## Local Deployment Requirements

### 1. Spin up the Caching Layer (Redis)
Launch the memory store in the background using Docker:
```bash
sudo docker run --name url-redis -p 6379:6379 -d redis
```

### 2. Restore Tools and Apply Database Migrations
Run these commands from the root directory to automatically restore local tools and build the SQLite database schema:
```bash
dotnet tool restore
dotnet tool run dotnet-ef database update --project UrlShortener.Api
```

### 3. Start the Server Pipeline
You can run the web application directly from the root without changing directories by targeting the project name:
```bash
dotnet run --project UrlShortener.Api
```
The server will boot up and listen locally on `http://localhost:5236`.
