# DockTheQueue — .NET API + RabbitMQ + MongoDB

Academic project demonstrating a minimal integration between a .NET 9 API, a RabbitMQ message queue, and a MongoDB database. The entire system is orchestrated exclusively with Docker Compose.

**GitHub: [DockTheQueue](https://github.com/t2ne/dockthequeue)**

## Overview

- The API exposes a single endpoint `POST /send` that accepts any valid JSON payload and publishes it to the RabbitMQ queue `jsonQueue`.
- The `QueueListener` service consumes messages from `jsonQueue` and stores each JSON document in MongoDB database `ApiMessages`, collection `ReceivedJson`.

High-level architecture:

```
[HTTP Client / Postman]
          |
          v
[API (/send)] --> [RabbitMQ (jsonQueue)] --> [QueueListener] --> [MongoDB (ApiMessages.ReceivedJson)]
```

## Technology Stack

- .NET 9 (Minimal API + console listener)
- RabbitMQ 3 (with Management UI)
- MongoDB 7 + Mongo Express
- Docker Compose for multi-service orchestration

## Prerequisites

- Docker & Docker Compose

## Run the System (Docker Only)

From the project root (`eqs/`):

```bash
docker compose up --build
```

Services exposed:

- API: http://localhost:5076
  - Endpoint: `POST /send`
  - Sample payload:
    ```json
    {
      "name": "Test User",
      "timestamp": "2025-11-11T20:00:00Z"
    }
    ```
- RabbitMQ UI: http://localhost:15672 (login: `guest` / `guest`)
- Mongo Express: http://localhost:8081 (login: `admin` / `admin`)

To stop all containers:

```bash
docker compose down
```

MongoDB data persists in the named Docker volume `mongo_data`.

## Ports

Default exposed ports:

- API: 5076
- RabbitMQ: 5672 (AMQP) / 15672 (UI)
- MongoDB: 27017
- Mongo Express: 8081

## Project Structure

```
eqs/
├── ApiRabbitMongo/        # API that publishes received JSON to RabbitMQ
│   ├── Program.cs
│   └── Dockerfile
├── QueueListener/         # Consumer that stores JSON documents in MongoDB
│   ├── Program.cs
│   └── Dockerfile
├── docker-compose.yml     # Service orchestration
└── README.md              # Documentation
```

## Processing Flow

1. Client sends `POST /send` with a JSON body.
2. API publishes the raw JSON string to RabbitMQ queue `jsonQueue`.
3. `QueueListener` consumes messages and attempts `BsonDocument.Parse`.
4. On success, the document is inserted into `ApiMessages.ReceivedJson` in MongoDB.

## Troubleshooting

| Issue                    | Possible Cause                       | Action                                                                                             |
| ------------------------ | ------------------------------------ | -------------------------------------------------------------------------------------------------- |
| API can't reach RabbitMQ | Startup race or wrong hostname       | Ensure container `rabbitmq` is healthy; check `RABBITMQ__HOSTNAME` env var in service definitions. |
| No documents in MongoDB  | Listener not running or parse errors | Check logs of `queuelistener` container; verify JSON validity.                                     |
| Port conflicts           | Existing local services              | Stop local RabbitMQ/Mongo or change published ports in `docker-compose.yml`.                       |

## Author

**- [t2ne](https://github.com/t2ne)**

---

Educational example of system integration using messaging and database persistence with .NET.
