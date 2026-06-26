# TCP Chat Program

![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)
![C#](https://img.shields.io/badge/C%23-13.0-239120)
![PostgreSQL](https://img.shields.io/badge/DB-PostgreSQL-336791)
![Docker](https://img.shields.io/badge/Deploy-Docker-2496ED)
![License](https://img.shields.io/badge/license-MIT-green)

A real-time TCP chat application built with .NET 10 and C#. Supports multiple concurrent clients, persistent message storage via PostgreSQL, and full Docker containerization.

---

## Stack

**.NET 10** · **C# 13** · **TCP Sockets** · **PostgreSQL** · **Docker** · **Docker Compose**

---

## Project Structure

```
Chat_program/
├── Chat.Client/        → Console client app (connects to server via TCP)
├── Chat.Server/        → TCP server (handles connections, broadcasts messages)
├── Chat.Shared/        → Shared models and message contracts
├── Chat_program/       → Solution root
├── docker-compose.yml  → Orchestrates PostgreSQL + Server containers
└── Chat_program.slnx   → Solution file
```

---

## Features

- Multi-client TCP connections with real-time broadcasting
- User join / leave notifications
- Persistent message storage in PostgreSQL
- Dockerized server and database (Docker Compose)
- Environment-variable-based client configuration (`SERVER_HOST`, `SERVER_PORT`)

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for running server + DB)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for running the client locally)

---

### Option 1 — Docker Stack + Local Client (Recommended)

Start the server and database in Docker:

```bash
docker-compose up
```

This starts:
- **PostgreSQL** on `localhost:5432`
- **TCP Chat Server** on `localhost:5000`

Then run the client from the build output:

```bash
cd Chat.Client/bin/Debug/net10.0
./Chat.Client.exe
```

Enter your username when prompted and start chatting.

---

### Option 2 — Visual Studio + Docker Backend

1. Start the Docker services in the background:

```bash
docker-compose up -d
```

2. Open `Chat_program.slnx` in Visual Studio.

3. Set `Chat.Client` as the startup project and press `F5`.

---

### Option 3 — Run Everything Locally (No Docker)

Requires a local PostgreSQL instance. Update the connection string in `Chat.Server`, then:

```bash
# Run the server
cd Chat.Server
dotnet run

# In a separate terminal, run the client
cd Chat.Client
dotnet run
```

---

## Configuration

The client reads connection settings from environment variables:

| Variable      | Default     | Description          |
|---------------|-------------|----------------------|
| `SERVER_HOST` | `127.0.0.1` | Server hostname / IP |
| `SERVER_PORT` | `5000`      | Server TCP port      |

Example:

```bash
SERVER_HOST=192.168.1.10 SERVER_PORT=5000 ./Chat.Client.exe
```

---

## Architecture

```
┌─────────────────────────────────────────────┐
│              Local Machine                  │
│                                             │
│  ┌──────────────────────────────────────┐   │
│  │       Docker Container Network       │   │
│  │                                      │   │
│  │  ┌──────────────┐  ┌──────────────┐  │   │
│  │  │  PostgreSQL  │  │  TCP Server  │  │   │
│  │  │  :5432       │◄─►  :5000       │  │   │
│  │  └──────────────┘  └──────┬───────┘  │   │
│  └─────────────────────────── │ ─────────┘   │
│                               │              │
│  ┌────────────────────────────▼───────────┐  │
│  │   Chat.Client  (Console App, local)    │  │
│  └────────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

---

## Troubleshooting

**`Connection refused` on client start**  
→ Make sure the server container is running: `docker-compose up`

**Docker containers fail to start**  
→ Remove volumes and restart: `docker-compose down -v && docker-compose up`

**Server can't reach the database**  
→ Check that Docker Desktop is running and the `postgres` container is healthy:

```bash
docker-compose ps
```

---

## License

[MIT](LICENSE.txt)
