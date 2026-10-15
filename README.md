# IntegratorAI

An AI integration platform built with **.NET 10** that exposes a clean REST API for chat completions and configurable AI contexts. It currently supports **HuggingFace** and **OpenRouter** as LLM providers and is designed to be easily extended with additional providers.

## Features

- 💬 **Chat completions** – create and continue multi-turn conversations
- 📡 **Streaming** – real-time token streaming via Server-Sent Events (SSE)
- 🧠 **Contexts** – define reusable AI personas with system roles, domain knowledge, decision policies, tools and few-shot examples
- 🔌 **Provider abstraction** – swap or extend LLM providers without touching business logic
- ⚡ **Redis caching** – configurable Redis-backed caching layer
- 🧩 **Modular architecture** – domain, application, infrastructure and persistence layers per bounded context

## Architecture

The solution follows a modular monolith approach with three main bounded contexts:

```
Source/
├── Integrator/          # ASP.NET Core API host (entry point)
├── Chat/                # Chat completions bounded context
├── Context/             # AI context management bounded context
├── Providers/           # LLM provider integration (HuggingFace, ...)
└── BuildingBlocks/      # Shared domain, application and infrastructure abstractions

Tests/
├── BuildingBlocks/      # Unit & integration tests for building blocks
├── Chat/                # Unit & integration tests for Chat
└── Providers/           # Unit & integration tests for Providers
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running **Redis** instance
- A **SQL Server** (or compatible) database
- A **HuggingFace API token** (`HF_API_KEY`) or an **OpenRouter API key** (`OPENROUTER_API_KEY`)

### Choosing the provider

The provider that answers is the one row of `providers.Provider` with `IsActive = 1` (`Type` is `HuggingFace` or `OpenRouter`, `PrimaryModel` is the model id, `SummarizationModel` is optional; OpenRouter uses the primary model when it is empty). The row is cached in Redis under `providers:active`, so after changing it delete that key (or wait for it to expire). A provider is available when it is listed under `Providers` in the settings (with its `BaseUrl`) and its API key is in the environment. Script `0002.ActivateOpenRouter.sql` makes OpenRouter the active provider (`google/gemma-4-31b-it:free`); to switch later, add a script that changes the row.

### Tool calling

A context can declare tools (name, description, parameters; a parameter can be `required`). On the streaming API the model may then ask for a tool instead of answering:

1. The stream carries `event: tool_call` with `data: {"id": "call_1", "name": "get_project", "arguments": {"id": 5}}` (one event per call) and ends with `data: [DONE]`. The completion now waits for the results.
2. The client runs the tools and sends the results to `POST /streamchat/completions/{id}/tool-results` with `{"results": [{"toolCallId": "call_1", "content": "...", "isError": false}]}`. Every call must be answered, exactly once.
3. The response is a stream again: the answer written from the results, or further `tool_call` events.

A completion that waits for results answers 409 to a new prompt, and 409 to results that do not match its calls. Providers decide how the tools reach the model: OpenRouter sends them as native `tools` (and `tool_choice: auto`); HuggingFace ignores them for now. Only the tools of the context are offered, so a client that handles the data itself can leave a tool out of the context.

### Configuration

Copy and adjust `appsettings.json` in `Source/Integrator/IntegratorAI.Api`:

```json
{
  "ConnectionStrings": {
    "IntegratorAI": "<your-connection-string>"
  },
  "Redis": {
    "ConnectionString": "<your-redis-connection-string>"
  },
  "Providers": [
    {
      "Type": "HuggingFace",
      "PrimaryModel": "<model-id>",
      "SummarizationModel": "<summarization-model-id>",
      "IsActive": true
    }
  ],
  "Chat": {
    "MaxTokens": 2048
  }
}
```

### Run

```bash
dotnet run --project Source/Integrator/IntegratorAI.Api
```

The API and interactive docs (Scalar) will be available at `https://localhost:<port>/scalar`.

### Build & Test

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test
```

## API Overview

### Context

| Method | Endpoint    | Description                          |
|--------|-------------|--------------------------------------|
| POST   | `/Context`  | Create a new AI context, returns its `Guid` |

Use the returned `Guid` as the `Context-Id` request header when creating a completion to shape the assistant's behavior.

### Chat (standard)

| Method | Endpoint                          | Description                        |
|--------|-----------------------------------|------------------------------------|
| POST   | `/Chat/completions`               | Create a new completion            |
| GET    | `/Chat/completions/{id}`          | Retrieve an existing completion    |
| POST   | `/Chat/completions/{id}`          | Continue an existing completion    |

### Chat (streaming — SSE)

| Method | Endpoint                                | Description                              |
|--------|-----------------------------------------|------------------------------------------|
| POST   | `/StreamChat/completions`               | Stream a new completion via SSE          |
| POST   | `/StreamChat/completions/{id}`          | Stream a continuation of a completion    |

## License

This project is licensed under the [MIT License](LICENSE).
