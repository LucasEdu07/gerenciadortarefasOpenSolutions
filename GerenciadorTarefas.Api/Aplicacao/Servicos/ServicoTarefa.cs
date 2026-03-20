using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ResultadoCriacaoTarefaResposta> CriarAsync(
        CriarTarefaRequisicao requisicao,
        string? chaveIdempotencia,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
        {
            var tarefaSemIdempotencia = Tarefa.Criar(requisicao.Titulo, requisicao.Descricao);

            contexto.Tarefas.Add(tarefaSemIdempotencia);
            await contexto.SaveChangesAsync(cancellationToken);

            return new ResultadoCriacaoTarefaResposta
            {
                Tarefa = tarefaSemIdempotencia.ParaResposta(),
                Reaproveitado = false
            };
        }

        var chaveNormalizada = chaveIdempotencia.Trim();
        var hashRequisicao = GerarHashRequisicao(requisicao);

        var registroExistente = await contexto.RegistrosIdempotencia
            .AsNoTracking()
            .FirstOrDefaultAsync(registro => registro.Chave == chaveNormalizada, cancellationToken);

        if (registroExistente is not null)
        {
            return ObterResultadoIdempotente(registroExistente, hashRequisicao);
        }

        var tarefa = Tarefa.Criar(requisicao.Titulo, requisicao.Descricao);
        var resposta = tarefa.ParaResposta();

        var registroIdempotencia = RegistroIdempotencia.Criar(
            chaveNormalizada,
            hashRequisicao,
            tarefa.Id,
            JsonSerializer.Serialize(resposta, OpcoesJson));

        contexto.Tarefas.Add(tarefa);
        contexto.RegistrosIdempotencia.Add(registroIdempotencia);

        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            contexto.ChangeTracker.Clear();

            var registroConcorrente = await contexto.RegistrosIdempotencia
                .AsNoTracking()
                .FirstOrDefaultAsync(registro => registro.Chave == chaveNormalizada, cancellationToken);

            if (registroConcorrente is null)
            {
                throw;
            }

            return ObterResultadoIdempotente(registroConcorrente, hashRequisicao);
        }

        return new ResultadoCriacaoTarefaResposta
        {
            Tarefa = resposta,
            Reaproveitado = false
        };
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
            ?? throw new RecursoNaoEncontradoException("Tarefa nao encontrada.");
    }

    private static string GerarHashRequisicao(CriarTarefaRequisicao requisicao)
    {
        var titulo = requisicao.Titulo.Trim();
        var descricao = string.IsNullOrWhiteSpace(requisicao.Descricao)
            ? string.Empty
            : requisicao.Descricao.Trim();

        var conteudoCanonico = $"{titulo}\n{descricao}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(conteudoCanonico));

        return Convert.ToHexString(bytes);
    }

    private static ResultadoCriacaoTarefaResposta ObterResultadoIdempotente(
        RegistroIdempotencia registro,
        string hashRequisicao)
    {
        if (!string.Equals(registro.HashRequisicao, hashRequisicao, StringComparison.Ordinal))
        {
            throw new ConflitoIdempotenciaException(
                "A chave de idempotencia informada ja foi utilizada com um payload diferente.");
        }

        var resposta = JsonSerializer.Deserialize<TarefaResposta>(registro.RespostaEmJson, OpcoesJson)
            ?? throw new InvalidOperationException("Nao foi possivel reconstruir a resposta idempotente.");

        return new ResultadoCriacaoTarefaResposta
        {
            Tarefa = resposta,
            Reaproveitado = true
        };
    }
}
