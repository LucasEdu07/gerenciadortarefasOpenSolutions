# Gerenciador de Tarefas

API REST em .NET 8 para gerenciamento de tarefas, com foco em organização, regras de negócio, validações, paginação, tratamento global de erros e clareza de código.

## Tecnologias

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- SQLite para persistência local
- Swagger para documentação interativa
- xUnit para testes unitários das principais regras

## Estrutura da solução

```text
GerenciadorTarefas.sln
|-- GerenciadorTarefas.Api
|   |-- Aplicacao
|   |   |-- DTOs
|   |   |-- Interfaces
|   |   |-- Mapeamentos
|   |   `-- Servicos
|   |-- Controllers
|   |-- Dominio
|   |   |-- Entidades
|   |   |-- Enumeradores
|   |   `-- Excecoes
|   `-- Infraestrutura
|       |-- Erros
|       `-- Persistencia
`-- GerenciadorTarefas.Testes
    `-- Dominio
```

## Decisões de implementação

- A entidade de domínio é `Tarefa`, com regras de status encapsuladas na própria entidade.
- O status usa `enum` e mantém os valores `Pending`, `InProgress` e `Done` para respeitar o contrato da API.
- O banco é SQLite e o arquivo `gerenciador-tarefas.db` é criado automaticamente na primeira execução.
- As respostas de erro seguem `ProblemDetails`, incluindo validações (`400`), recurso não encontrado (`404`) e regra de negócio (`422`).
- A listagem é sempre paginada e aceita filtro opcional por status.
- Os nomes internos do código estão em português, enquanto o contrato HTTP mantém os campos esperados pelo desafio, como `title`, `description`, `createdAt` e `updatedAt`.

## Como executar

### Pré-requisitos

- SDK do .NET 8 ou superior instalado

### Passos

```bash
dotnet restore .\GerenciadorTarefas.sln
dotnet build .\GerenciadorTarefas.Api\GerenciadorTarefas.Api.csproj --no-restore
dotnet run --project .\GerenciadorTarefas.Api\GerenciadorTarefas.Api.csproj
```

Com a API em execução:

- Swagger: `http://localhost:5187/swagger`
- Base URL padrão: `http://localhost:5187`

## Como rodar os testes

```bash
dotnet build .\GerenciadorTarefas.Testes\GerenciadorTarefas.Testes.csproj --no-restore
dotnet test .\GerenciadorTarefas.Testes\GerenciadorTarefas.Testes.csproj --no-build
```

## Endpoints

- `POST /api/tasks`
- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

## Regras de negócio atendidas

- Toda nova tarefa inicia com status `Pending`
- O título é obrigatório
- `createdAt` e `updatedAt` são gerados apenas pelo sistema
- O fluxo de status é linear: `Pending -> InProgress -> Done`
- Não é permitido regredir status
- Não é permitido pular etapas
- Após chegar em `Done`, a tarefa não permite nova alteração de status
- A listagem funciona com paginação obrigatória e filtro por status

## Exemplos de request/response

### Criar tarefa

`POST /api/tasks`

```json
{
  "title": "Preparar teste técnico",
  "description": "Modelar entidade, regras e paginação."
}
```

Resposta `201 Created`:

```json
{
  "id": "8a7f62ab-69d8-4dca-a5f8-9d6d6d7dd4aa",
  "title": "Preparar teste técnico",
  "description": "Modelar entidade, regras e paginação.",
  "status": "Pending",
  "createdAt": "2026-03-20T18:30:25.8314817Z",
  "updatedAt": "2026-03-20T18:30:25.8314817Z"
}
```

### Listar tarefas paginadas

`GET /api/tasks?pageNumber=1&pageSize=10&status=Pending`

Resposta `200 OK`:

```json
{
  "items": [
    {
      "id": "8a7f62ab-69d8-4dca-a5f8-9d6d6d7dd4aa",
      "title": "Preparar teste técnico",
      "description": "Modelar entidade, regras e paginação.",
      "status": "Pending",
      "createdAt": "2026-03-20T18:30:25.8314817Z",
      "updatedAt": "2026-03-20T18:30:25.8314817Z"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

### Atualizar status corretamente

`PUT /api/tasks/{id}`

```json
{
  "title": "Preparar teste técnico",
  "description": "Modelar entidade, regras, paginação e testes.",
  "status": "InProgress"
}
```

Resposta `200 OK`:

```json
{
  "id": "8a7f62ab-69d8-4dca-a5f8-9d6d6d7dd4aa",
  "title": "Preparar teste técnico",
  "description": "Modelar entidade, regras, paginação e testes.",
  "status": "InProgress",
  "createdAt": "2026-03-20T18:30:25.8314817Z",
  "updatedAt": "2026-03-20T18:35:10.1954202Z"
}
```

### Erro de regra de negócio

Tentativa de avançar de `Pending` direto para `Done`.

Resposta `422 Unprocessable Entity`:

```json
{
  "type": "about:blank",
  "title": "Violação de regra de negócio",
  "status": 422,
  "detail": "Não é permitido avançar o status pulando etapas.",
  "instance": "/api/tasks/8a7f62ab-69d8-4dca-a5f8-9d6d6d7dd4aa",
  "traceId": "00-b6e5f6c0d25d5e4a95f338f9eb1baf42-5a0e1943fd2083d7-00"
}
```

### Erro de validação

Criação sem título.

Resposta `400 Bad Request`:

```json
{
  "title": "Falha de validação",
  "status": 400,
  "detail": "Um ou mais campos da requisição estão inválidos.",
  "errors": {
    "title": [
      "O título da tarefa é obrigatório."
    ]
  }
}
```

## Melhorias futuras

- Adicionar migrations versionadas do EF Core em vez de `EnsureCreated`
- Cobrir a API com testes de integração além dos testes unitários de domínio
- Incluir observabilidade mais robusta, como logs estruturados e métricas
