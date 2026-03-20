using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;

public sealed class CriarTarefaRequisicao
{
    [Required(ErrorMessage = "O título da tarefa é obrigatório.")]
    [JsonPropertyName("title")]
    public string Titulo { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; init; }
}
