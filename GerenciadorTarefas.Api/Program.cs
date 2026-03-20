using System.Text.Json.Serialization;
using GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;
using GerenciadorTarefas.Api.Aplicacao.Interfaces;
using GerenciadorTarefas.Api.Aplicacao.Servicos;
using GerenciadorTarefas.Api.Dominio.Enumeradores;
using GerenciadorTarefas.Api.Infraestrutura.Erros;
using GerenciadorTarefas.Api.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var detalhes = new ValidationProblemDetails(contexto.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Falha de validação",
            Detail = "Um ou mais campos da requisição estão inválidos.",
            Instance = contexto.HttpContext.Request.Path
        };

        detalhes.Extensions["traceId"] = contexto.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(detalhes)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(configuracao =>
{
    configuracao.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gerenciamento de Tarefas",
        Version = "v1",
        Description = "API REST para criação, consulta, atualização e remoção de tarefas."
    });

    configuracao.MapType<StatusTarefa>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum
            .GetNames<StatusTarefa>()
            .Select(status => (IOpenApiAny)new OpenApiString(status))
            .ToList()
    });

    configuracao.MapType<StatusTarefa?>(() => new OpenApiSchema
    {
        Type = "string",
        Nullable = true,
        Enum = Enum
            .GetNames<StatusTarefa>()
            .Select(status => (IOpenApiAny)new OpenApiString(status))
            .ToList()
    });
});

builder.Services.AddDbContext<ContextoAplicacao>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("BancoTarefas")));

builder.Services.AddScoped<IServicoTarefa, ServicoTarefa>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<MiddlewareTratamentoExcecoes>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
    contexto.Database.EnsureCreated();
    contexto.Database.ExecuteSqlRaw(
        """
        CREATE TABLE IF NOT EXISTS "RegistrosIdempotencia" (
            "Chave" TEXT NOT NULL CONSTRAINT "PK_RegistrosIdempotencia" PRIMARY KEY,
            "HashRequisicao" TEXT NOT NULL,
            "TarefaId" TEXT NOT NULL,
            "RespostaEmJson" TEXT NOT NULL,
            "CriadoEm" TEXT NOT NULL
        );
        """);
    contexto.Database.ExecuteSqlRaw(
        """
        CREATE INDEX IF NOT EXISTS "IX_RegistrosIdempotencia_TarefaId"
        ON "RegistrosIdempotencia" ("TarefaId");
        """);
}

app.MapControllers();

app.Run();

public partial class Program;
