using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GerenciadorTarefas.Api.Dominio.Enumeradores;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;

public sealed class AtualizarTarefaRequisicao
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório.")]
    [JsonPropertyName("title")]
    public string Titulo { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; init; }

    [JsonPropertyName("status")]
    public StatusTarefa? Status { get; init; }
}
