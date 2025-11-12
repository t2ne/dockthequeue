# Projeto EQS — Integração RabbitMQ + MongoDB + API .NET

Este projeto demonstra uma integração simples entre .NET 9, RabbitMQ e MongoDB, utilizando Docker Compose para orquestrar todos os serviços.

## Estrutura do Projeto

eqs/
│
├── ApiRabbitMongo/         # API que recebe JSONs e envia para a fila RabbitMQ
│   └── Program.cs
│
├── QueueListener/          # Serviço que lê mensagens da fila e guarda no MongoDB
│   └── Program.cs
│
├── docker-compose.yml      # Define e liga todos os serviços (API, listener, RabbitMQ, MongoDB)
│
└── README.md               # Este manual

## Iniciar o Projeto com Docker

### 1. Construir e arrancar todos os serviços

No diretório raiz (eqs/), executa:

```bash
docker compose up --build
```

Isto vai arrancar:
- MongoDB → Base de dados onde as mensagens são guardadas
- Mongo Express → Interface web para visualizar a base de dados
- RabbitMQ → Fila de mensagens
- ApiRabbitMongo → API que envia JSONs para a fila
- QueueListener → Serviço que consome mensagens e insere no MongoDB

### 2. Aceder aos serviços

- **API:** http://localhost:5076  
  Endpoint: `POST /send`  
  Exemplo de corpo JSON:
  ```json
  {
      "name": "Utilizador Teste",
      "timestamp": "2025-11-11T20:00:00Z"
  }
  ```

- **RabbitMQ Management UI:** http://localhost:15672  
  Login: `guest` / `guest`

- **Mongo Express (UI da base de dados):** http://localhost:8081

### 3. Verificar os dados guardados

Na interface do **Mongo Express**, abre a base de dados `ApiMessages` e a coleção `ReceivedJson`.  
Deverás ver o documento com os dados JSON enviados pela API.

### 4. Parar e limpar tudo

Para parar os serviços:

```bash
docker compose down
```

Para remover todos os containers e imagens Docker (atenção — isto apaga tudo):

```bash
docker system prune -a
```

## Estrutura das Componentes

### ApiRabbitMongo (API)
- Recebe pedidos `POST` em `/send`
- Lê o corpo JSON
- Publica a mensagem na fila RabbitMQ (`jsonQueue`)

### QueueListener (Consumidor)
- Escuta mensagens na fila RabbitMQ
- Insere os documentos recebidos na coleção `ReceivedJson` dentro da base `ApiMessages`

## Requisitos

- Docker e Docker Compose instalados
- .NET 9 SDK (apenas se quiseres executar localmente sem Docker)

## Execução Local (sem Docker)

1. Inicia manualmente o MongoDB e o RabbitMQ (por exemplo, através do Docker Desktop)
2. Executa a API:
   ```bash
   cd ApiRabbitMongo
   dotnet run
   ```
3. Noutro terminal, executa o consumidor:
   ```bash
   cd QueueListener
   dotnet run
   ```

---

Projeto desenvolvido como exemplo educativo de integração entre mensageria e base de dados com .NET.
