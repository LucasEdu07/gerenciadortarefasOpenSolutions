using GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;
using GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;
using GerenciadorTarefas.Api.Aplicacao.Interfaces;
using GerenciadorTarefas.Api.Aplicacao.Mapeamentos;
using GerenciadorTarefas.Api.Dominio.Entidades;
using GerenciadorTarefas.Api.Dominio.Excecoes;
using GerenciadorTarefas.Api.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Api.Aplicacao.Servicos;

public sealed class ServicoTarefa(ContextoAplicacao contexto) : IServicoTarefa
{
    public async Task<TarefaResposta> CriarAsync(
        CriarTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var tarefa = Tarefa.Criar(requisicao.Titulo, requisicao.Descricao);

        contexto.Tarefas.Add(tarefa);
        await contexto.SaveChangesAsync(cancellationToken);

        return tarefa.ParaResposta();
    }

    public async Task<ListaPaginadaResposta<TarefaResposta>> ListarAsync(
        ConsultaTarefasRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var consulta = contexto.Tarefas.AsNoTracking().AsQueryable();

        if (requisicao.Status.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.Status == requisicao.Status.Value);
        }

        var totalItens = await consulta.CountAsync(cancellationToken);

        var tarefas = await consulta
            .OrderByDescending(tarefa => tarefa.CriadoEm)
            .Skip((requisicao.NumeroPagina - 1) * requisicao.TamanhoPagina)
            .Take(requisicao.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return new ListaPaginadaResposta<TarefaResposta>
        {
            Itens = tarefas.Select(tarefa => tarefa.ParaResposta()).ToArray(),
            Paginacao = new MetadadosPaginacaoResposta
            {
                NumeroPagina = requisicao.NumeroPagina,
                TamanhoPagina = requisicao.TamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalItens == 0
                    ? 0
                    : (int)Math.Ceiling(totalItens / (double)requisicao.TamanhoPagina)
            }
        };
    }

    public async Task<TarefaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tarefa = await ObterTarefaAsync(id, cancellationToken);
        return tarefa.ParaResposta();
    }

    public async Task<TarefaResposta> AtualizarAsync(
        Guid id,
        AtualizarTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var tarefa = await ObterTarefaAsync(id, cancellationToken);

        tarefa.AtualizarDetalhes(requisicao.Titulo, requisicao.Descricao);

        if (requisicao.Status.HasValue)
        {
            tarefa.AtualizarStatus(requisicao.Status.Value);
        }

        await contexto.SaveChangesAsync(cancellationToken);

        return tarefa.ParaResposta();
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken)
    {
        var tarefa = await ObterTarefaAsync(id, cancellationToken);

        contexto.Tarefas.Remove(tarefa);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task<Tarefa> ObterTarefaAsync(Guid id, CancellationToken cancellationToken)
    {
        return await contexto.Tarefas.FirstOrDefaultAsync(tarefa => tarefa.Id == id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Tarefa não encontrada.");
    }
}
