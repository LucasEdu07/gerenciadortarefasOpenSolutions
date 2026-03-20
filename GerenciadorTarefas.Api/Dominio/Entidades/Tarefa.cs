using GerenciadorTarefas.Api.Dominio.Enumeradores;
using GerenciadorTarefas.Api.Dominio.Excecoes;

namespace GerenciadorTarefas.Api.Dominio.Entidades;

public class Tarefa
{
    private Tarefa()
    {
    }

    private Tarefa(string titulo, string? descricao)
    {
        ValidarTitulo(titulo);

        Id = Guid.NewGuid();
        Titulo = titulo.Trim();
        Descricao = NormalizarDescricao(descricao);
        Status = StatusTarefa.Pending;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public Guid Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public StatusTarefa Status { get; private set; }

    public DateTime CriadoEm { get; private set; }

    public DateTime AtualizadoEm { get; private set; }

    public static Tarefa Criar(string titulo, string? descricao)
    {
        return new Tarefa(titulo, descricao);
    }

    public void AtualizarDetalhes(string titulo, string? descricao)
    {
        ValidarTitulo(titulo);

        Titulo = titulo.Trim();
        Descricao = NormalizarDescricao(descricao);
        AtualizarData();
    }

    public void AtualizarStatus(StatusTarefa novoStatus)
    {
        if (novoStatus == Status)
        {
            return;
        }

        if (Status == StatusTarefa.Done)
        {
            throw new RegraDeNegocioException("Tarefas finalizadas não permitem mais alteração de status.");
        }

        var diferencaEntreStatus = (int)novoStatus - (int)Status;

        if (diferencaEntreStatus < 0)
        {
            throw new RegraDeNegocioException("Não é permitido regredir o status da tarefa.");
        }

        if (diferencaEntreStatus > 1)
        {
            throw new RegraDeNegocioException("Não é permitido avançar o status pulando etapas.");
        }

        Status = novoStatus;
        AtualizarData();
    }

    private void AtualizarData()
    {
        AtualizadoEm = DateTime.UtcNow;
    }

    private static void ValidarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new RegraDeNegocioException("O título da tarefa é obrigatório.");
        }
    }

    private static string? NormalizarDescricao(string? descricao)
    {
        return string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
    }
}
