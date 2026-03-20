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
            logger.LogError(exception, "Erro não tratado durante o processamento da requisição.");
            await EscreverRespostaAsync(contexto, exception);
        }
    }

    private static async Task EscreverRespostaAsync(HttpContext contexto, Exception exception)
    {
        var (codigoStatus, titulo, detalhe) = exception switch
        {
            RecursoNaoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),
            RegraDeNegocioException => (
                StatusCodes.Status422UnprocessableEntity,
                "Violação de regra de negócio",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu um erro inesperado ao processar a requisição.")
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
