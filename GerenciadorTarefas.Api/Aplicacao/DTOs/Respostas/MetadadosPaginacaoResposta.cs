using System.Text.Json.Serialization;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;

public sealed class MetadadosPaginacaoResposta
{
    [JsonPropertyName("pageNumber")]
    public int NumeroPagina { get; init; }

    [JsonPropertyName("pageSize")]
    public int TamanhoPagina { get; init; }

    [JsonPropertyName("totalItems")]
    public int TotalItens { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPaginas { get; init; }
}
