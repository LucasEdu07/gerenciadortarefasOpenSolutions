using GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;
using GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;
using GerenciadorTarefas.Api.Aplicacao.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TarefasController(IServicoTarefa servicoTarefa) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TarefaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TarefaResposta>> Criar(
        [FromBody] CriarTarefaRequisicao requisicao,
        [FromHeader(Name = "Idempotency-Key")] string? chaveIdempotencia,
        CancellationToken cancellationToken)
    {
        var resultado = await servicoTarefa.CriarAsync(requisicao, chaveIdempotencia, cancellationToken);

        if (resultado.Reaproveitado)
        {
            Response.Headers.Append("Idempotency-Replayed", "true");
            return Ok(resultado.Tarefa);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Tarefa.Id }, resultado.Tarefa);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ListaPaginadaResposta<TarefaResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListaPaginadaResposta<TarefaResposta>>> Listar(
        [FromQuery] ConsultaTarefasRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var resposta = await servicoTarefa.ListarAsync(requisicao, cancellationToken);
        return Ok(resposta);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TarefaResposta>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resposta = await servicoTarefa.ObterPorIdAsync(id, cancellationToken);
        return Ok(resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TarefaResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TarefaResposta>> Atualizar(
        Guid id,
        [FromBody] AtualizarTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var resposta = await servicoTarefa.AtualizarAsync(id, requisicao, cancellationToken);
        return Ok(resposta);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await servicoTarefa.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
