# Population Council API - Docker Setup

This project is a `.NET 6.0` Web API application. This repository contains the configuration to build, run, and test the entire project locally using a containerized environment.

---

## Architecture Overview

The containerized environment includes:
- **`api`**: The ASP.NET Core 6.0 Web API container.
- **`db`**: Microsoft SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`).
- **`redis`**: Redis Alpine container used for distributed caching.
- **`db-init`**: An ephemeral container that installs `sqlpackage` and imports the database schema and lookup tables from a `.bacpac` backup file on initial startup.

```
                    ┌───────────────────┐
                    │  Docker Compose   │
                    └─────────┬─────────┘
                              │
         ┌────────────────────┼────────────────────┐
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│     pc_redis    │  │   pc_database   │  │pc_db_initializer│
│ (Redis Cache)   │  │  (SQL Server)   │  │  (sqlpackage)   │
└────────┬────────┘  └────────┬────────┘  └────────┬────────┘
         │                    ▲                    │
         │                    │                    │
         │                    │ Wait / Import DB   │
         │                    └────────────────────┘
         │                    ▲
         │ Connect            │ Connect
         └──────────┐ ┌───────┘
                    │ │
                 ┌──┴─┴──────┐
                 │pc_web_api │
                 │(Web API)  │
                 └───────────┘
```

---

## Prerequisites

Ensure you have the following installed:
- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

---

## Getting Started

### 1. Configure the Environment Settings

A default `.env` file is generated in the root of the project. You can edit this file to change passwords, ports, or select a different database backup to restore:

```ini
# SQL Server Configuration
MSSQL_SA_PASSWORD=PopulationCouncil2026!
DB_NAME=PopulationCouncilDB
DB_PORT=1433

# Database Backup File to Restore (Default is recommended)
BACPAC_PATH=Database Backup/Live Database/livePopulationCouncilDb.bacpac

# Web API External Port
API_PORT=5000
```

### 2. Start the Application

Run the following command at the root of the project to build the API and start all services:

```bash
docker compose up --build
```

### 3. What happens during startup?

1. **`db` and `redis` start up:** SQL Server launches and initializes its database engine.
2. **`db-init` waits for SQL Server:** The database initializer waits until the SQL Server port `1433` is ready.
3. **Database Restore:** Once ready, the initializer uses `sqlpackage` to restore the `.bacpac` file to SQL Server. After completion, it writes a marker file (`.import_completed`) to ensure it only imports once.
4. **`api` launches:** Once the database restore succeeds, the API container builds, runs, and binds to port `5000` (by default).

---

## Verification

### Swagger UI (Web API Docs)
Open your browser and navigate to:
```
http://localhost:5000/swagger
```
Here, you should see the Swagger UI listing all active API endpoints.

### SQL Server Connection
You can connect to the database using any SQL Client (e.g., Azure Data Studio, DBeaver, or SSMS):
- **Server/Host**: `localhost,1433`
- **Database**: `PopulationCouncilDB`
- **Username**: `sa`
- **Password**: `PopulationCouncil2026!` (or whatever is in `.env`)
- **Trust Server Certificate**: `True`

### Redis Cache
To verify the cache is active, you can inspect the Redis container logs or use the Redis CLI:
```bash
docker exec -it pc_redis redis-cli ping
```
*Expected response: `PONG`*

---

## Troubleshooting

### Database Initializer Fails or Hangs
If the `db-init` container fails:
1. Inspect the logs to see what failed:
   ```bash
   docker compose logs db-init
   ```
2. If you need to force a database re-import, delete the marker file and restart:
   ```bash
   # Remove database files and markers in the volume
   docker compose down -v
   docker compose up --build
   ```

### Port Conflicts
If you receive an error like `port is already allocated`:
- Open `.env` and change `API_PORT`, `DB_PORT`, or `REDIS_PORT` to an unused port.
- Restart the containers.
