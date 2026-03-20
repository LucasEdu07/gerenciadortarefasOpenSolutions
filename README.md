# Gerenciador de Tarefas

API REST em .NET 8 para gerenciamento de tarefas, desenvolvida como teste técnico com foco em clareza, regras de negócio, validações, paginação e tratamento consistente de erros.

## Tecnologias

- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger
- xUnit

## Como executar

No diretório raiz do projeto:

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

## Regras de negócio

- O título é obrigatório
- Toda nova tarefa inicia com status `Pending`
- O fluxo de status é linear: `Pending -> InProgress -> Done`
- Não é permitido regredir status
- Não é permitido pular etapas
- Tarefas em `Done` não permitem nova alteração de status
- `createdAt` e `updatedAt` são gerados pelo sistema
- A listagem é paginada e aceita filtro por status

## Observações

- O banco utilizado é SQLite e é criado automaticamente na primeira execução
- Os nomes internos do código estão em português
- O contrato da API mantém os campos esperados no desafio, como `title`, `description`, `createdAt` e `updatedAt`
