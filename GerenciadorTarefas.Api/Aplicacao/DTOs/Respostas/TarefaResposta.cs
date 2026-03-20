using System.Text.Json.Serialization;
using GerenciadorTarefas.Api.Dominio.Enumeradores;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;

public sealed class TarefaResposta
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Titulo { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Descricao { get; init; }

    [JsonPropertyName("status")]
    public StatusTarefa Status { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CriadoEm { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTime AtualizadoEm { get; init; }
}
