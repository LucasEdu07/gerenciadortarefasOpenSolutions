namespace GerenciadorTarefas.Api.Dominio.Entidades;

public class RegistroIdempotencia
{
    private RegistroIdempotencia()
    {
    }

    private RegistroIdempotencia(
        string chave,
        string hashRequisicao,
        Guid tarefaId,
        string respostaEmJson)
    {
        Chave = chave;
        HashRequisicao = hashRequisicao;
        TarefaId = tarefaId;
        RespostaEmJson = respostaEmJson;
        CriadoEm = DateTime.UtcNow;
    }

    public string Chave { get; private set; } = string.Empty;

    public string HashRequisicao { get; private set; } = string.Empty;

    public Guid TarefaId { get; private set; }

    public string RespostaEmJson { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; }

    public static RegistroIdempotencia Criar(
        string chave,
        string hashRequisicao,
        Guid tarefaId,
        string respostaEmJson)
    {
        return new RegistroIdempotencia(chave, hashRequisicao, tarefaId, respostaEmJson);
    }
}
