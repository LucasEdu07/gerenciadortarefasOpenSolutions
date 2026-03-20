using GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;
using GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;

namespace GerenciadorTarefas.Api.Aplicacao.Interfaces;

public interface IServicoTarefa
{
    Task<ResultadoCriacaoTarefaResposta> CriarAsync(
        CriarTarefaRequisicao requisicao,
        string? chaveIdempotencia,
        CancellationToken cancellationToken);

    Task<ListaPaginadaResposta<TarefaResposta>> ListarAsync(
        ConsultaTarefasRequisicao requisicao,
        CancellationToken cancellationToken);

    Task<TarefaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TarefaResposta> AtualizarAsync(
        Guid id,
        AtualizarTarefaRequisicao requisicao,
        CancellationToken cancellationToken);

    Task RemoverAsync(Guid id, CancellationToken cancellationToken);
}
