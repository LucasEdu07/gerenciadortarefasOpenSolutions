using GerenciadorTarefas.Api.Dominio.Entidades;
using GerenciadorTarefas.Api.Dominio.Enumeradores;
using GerenciadorTarefas.Api.Dominio.Excecoes;

namespace GerenciadorTarefas.Testes.Dominio;

public sealed class TarefaTestes
{
    [Fact]
    public void Criar_DeveIniciarComStatusPendingEDatasGeradas()
    {
        var tarefa = Tarefa.Criar("Estudar ASP.NET Core", "Revisar controllers e middlewares.");

        Assert.NotEqual(Guid.Empty, tarefa.Id);
        Assert.Equal("Estudar ASP.NET Core", tarefa.Titulo);
        Assert.Equal("Revisar controllers e middlewares.", tarefa.Descricao);
        Assert.Equal(StatusTarefa.Pending, tarefa.Status);
        Assert.NotEqual(default, tarefa.CriadoEm);
        Assert.Equal(tarefa.CriadoEm, tarefa.AtualizadoEm);
    }

    [Fact]
    public void AtualizarStatus_DevePermitirFluxoLinear()
    {
        var tarefa = Tarefa.Criar("Implementar endpoint", null);

        tarefa.AtualizarStatus(StatusTarefa.InProgress);
        tarefa.AtualizarStatus(StatusTarefa.Done);

        Assert.Equal(StatusTarefa.Done, tarefa.Status);
    }

    [Fact]
    public void AtualizarStatus_DeveImpedirSaltoDeEtapa()
    {
        var tarefa = Tarefa.Criar("Implementar endpoint", null);

        var excecao = Assert.Throws<RegraDeNegocioException>(() => tarefa.AtualizarStatus(StatusTarefa.Done));

        Assert.Equal("Não é permitido avançar o status pulando etapas.", excecao.Message);
        Assert.Equal(StatusTarefa.Pending, tarefa.Status);
    }

    [Fact]
    public void AtualizarStatus_DeveImpedirRegressao()
    {
        var tarefa = Tarefa.Criar("Implementar endpoint", null);
        tarefa.AtualizarStatus(StatusTarefa.InProgress);

        var excecao = Assert.Throws<RegraDeNegocioException>(() => tarefa.AtualizarStatus(StatusTarefa.Pending));

        Assert.Equal("Não é permitido regredir o status da tarefa.", excecao.Message);
        Assert.Equal(StatusTarefa.InProgress, tarefa.Status);
    }

    [Fact]
    public void AtualizarStatus_DeveImpedirMudancaQuandoTarefaEstiverConcluida()
    {
        var tarefa = Tarefa.Criar("Implementar endpoint", null);
        tarefa.AtualizarStatus(StatusTarefa.InProgress);
        tarefa.AtualizarStatus(StatusTarefa.Done);

        var excecao = Assert.Throws<RegraDeNegocioException>(() => tarefa.AtualizarStatus(StatusTarefa.InProgress));

        Assert.Equal("Tarefas finalizadas não permitem mais alteração de status.", excecao.Message);
        Assert.Equal(StatusTarefa.Done, tarefa.Status);
    }

    [Fact]
    public void AtualizarDetalhes_DeveExigirTitulo()
    {
        var tarefa = Tarefa.Criar("Implementar endpoint", null);

        var excecao = Assert.Throws<RegraDeNegocioException>(() => tarefa.AtualizarDetalhes("   ", "Descrição"));

        Assert.Equal("O título da tarefa é obrigatório.", excecao.Message);
    }
}
