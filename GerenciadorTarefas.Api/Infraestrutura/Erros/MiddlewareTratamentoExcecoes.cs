using GerenciadorTarefas.Api.Dominio.Excecoes;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Infraestrutura.Erros;

public sealed class MiddlewareTratamentoExcecoes(
    RequestDelegate proximo,
    ILogger<MiddlewareTratamentoExcecoes> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await proximo(contexto);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro nao tratado durante o processamento da requisicao.");
            await EscreverRespostaAsync(contexto, exception);
        }
    }

    private static async Task EscreverRespostaAsync(HttpContext contexto, Exception exception)
    {
        var (codigoStatus, titulo, detalhe) = exception switch
        {
            ConflitoIdempotenciaException => (
                StatusCodes.Status409Conflict,
                "Conflito de idempotencia",
                exception.Message),
            RecursoNaoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso nao encontrado",
                exception.Message),
            RegraDeNegocioException => (
                StatusCodes.Status422UnprocessableEntity,
                "Violacao de regra de negocio",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu um erro inesperado ao processar a requisicao.")
        };

        var detalhesProblema = new ProblemDetails
        {
            Status = codigoStatus,
            Title = titulo,
            Detail = detalhe,
            Instance = contexto.Request.Path
        };

        detalhesProblema.Extensions["traceId"] = contexto.TraceIdentifier;

        contexto.Response.StatusCode = codigoStatus;
        contexto.Response.ContentType = "application/problem+json";

        await contexto.Response.WriteAsJsonAsync(detalhesProblema);
    }
}
