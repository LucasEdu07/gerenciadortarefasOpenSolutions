namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;

public sealed class ResultadoCriacaoTarefaResposta
{
    public TarefaResposta Tarefa { get; init; } = new();

    public bool Reaproveitado { get; init; }
}
