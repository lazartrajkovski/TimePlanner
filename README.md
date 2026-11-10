# TimePlanner

TimePlanner is an application for organizing assignments and tracking time.

## Local Development

Docker and Docker Compose are required.

Create a `.env` file based on `.env.example`, then set your PostgreSQL password and JWT signing key.

Start the application:

```bash
docker compose up -d --build
```

The API will be available at `http://localhost:8080`.

Stop the containers with:

```bash
docker compose down
```