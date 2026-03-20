# Gerenciador de Tarefas

API REST em .NET 8 para gerenciamento de tarefas, desenvolvida como teste tecnico com foco em clareza, regras de negocio, validacoes, paginacao, tratamento consistente de erros e idempotencia no endpoint de criacao.

## Tecnologias

- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger
- xUnit

## Como executar

No diretorio raiz do projeto:

```bash
dotnet restore .\GerenciadorTarefas.sln
dotnet run --project .\GerenciadorTarefas.Api\GerenciadorTarefas.Api.csproj
```

Swagger:

```text
http://localhost:5187/swagger
```

## Como rodar os testes

```bash
dotnet test .\GerenciadorTarefas.Testes\GerenciadorTarefas.Testes.csproj
```

## Endpoints

- `POST /api/tasks`
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

## Regras de negocio

- O titulo e obrigatorio
- Toda nova tarefa inicia com status `Pending`
- O fluxo de status e linear: `Pending -> InProgress -> Done`
- Nao e permitido regredir status
- Nao e permitido pular etapas
- Tarefas em `Done` nao permitem nova alteracao de status
- O `POST /api/tasks` aceita o header `Idempotency-Key`
- `createdAt` e `updatedAt` sao gerados pelo sistema
- A listagem e paginada e aceita filtro por status

## Observacoes

- O banco utilizado e SQLite e e criado automaticamente na primeira execucao
- Os nomes internos do codigo estao em portugues
- O contrato da API mantem os campos esperados no desafio, como `title`, `description`, `createdAt` e `updatedAt`
- Repetir o `POST` com a mesma `Idempotency-Key` e o mesmo payload reutiliza a criacao anterior
- Repetir a mesma `Idempotency-Key` com payload diferente retorna `409 Conflict`
