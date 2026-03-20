using GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;
using GerenciadorTarefas.Api.Aplicacao.Servicos;
using GerenciadorTarefas.Api.Dominio.Excecoes;
using GerenciadorTarefas.Api.Infraestrutura.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Testes.Aplicacao;

public sealed class ServicoTarefaTestes : IDisposable
{
    private readonly SqliteConnection conexao;
    private readonly ContextoAplicacao contexto;
    private readonly ServicoTarefa servicoTarefa;

    public ServicoTarefaTestes()
    {
        conexao = new SqliteConnection("Data Source=:memory:");
        conexao.Open();

        var opcoes = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlite(conexao)
            .Options;

        contexto = new ContextoAplicacao(opcoes);
        contexto.Database.EnsureCreated();

        servicoTarefa = new ServicoTarefa(contexto);
    }

    [Fact]
    public async Task CriarAsync_ComMesmaChaveERequisicao_DeveReaproveitarCriacaoAnterior()
    {
        var requisicao = new CriarTarefaRequisicao
        {
            Titulo = "Implementar idempotencia",
            Descricao = "Garantir reuso da mesma criacao."
        };

        var primeiraResposta = await servicoTarefa.CriarAsync(requisicao, "chave-idempotente-1", CancellationToken.None);
        var segundaResposta = await servicoTarefa.CriarAsync(requisicao, "chave-idempotente-1", CancellationToken.None);

        Assert.False(primeiraResposta.Reaproveitado);
        Assert.True(segundaResposta.Reaproveitado);
        Assert.Equal(primeiraResposta.Tarefa.Id, segundaResposta.Tarefa.Id);
        Assert.Equal(primeiraResposta.Tarefa.Titulo, segundaResposta.Tarefa.Titulo);
        Assert.Equal(1, await contexto.Tarefas.CountAsync());
        Assert.Equal(1, await contexto.RegistrosIdempotencia.CountAsync());
    }

    [Fact]
    public async Task CriarAsync_ComMesmaChaveEPayloadDiferente_DeveLancarConflito()
    {
        await servicoTarefa.CriarAsync(
            new CriarTarefaRequisicao
            {
                Titulo = "Tarefa original",
                Descricao = "Primeira versao"
            },
            "chave-idempotente-2",
            CancellationToken.None);

        var acao = () => servicoTarefa.CriarAsync(
            new CriarTarefaRequisicao
            {
                Titulo = "Tarefa alterada",
                Descricao = "Segunda versao"
            },
            "chave-idempotente-2",
            CancellationToken.None);

        var excecao = await Assert.ThrowsAsync<ConflitoIdempotenciaException>(acao);

        Assert.Equal(
            "A chave de idempotencia informada ja foi utilizada com um payload diferente.",
            excecao.Message);
    }

    public void Dispose()
    {
        contexto.Dispose();
        conexao.Dispose();
    }
}
