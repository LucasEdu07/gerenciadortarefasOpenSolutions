using System.Text.Json.Serialization;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;

public sealed class ListaPaginadaResposta<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyCollection<T> Itens { get; init; } = Array.Empty<T>();

    [JsonPropertyName("pagination")]
    public MetadadosPaginacaoResposta Paginacao { get; init; } = new();
}
