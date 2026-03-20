using GerenciadorTarefas.Api.Aplicacao.DTOs.Respostas;
using GerenciadorTarefas.Api.Dominio.Entidades;

namespace GerenciadorTarefas.Api.Aplicacao.Mapeamentos;

public static class TarefaMapeamentos
{
    public static TarefaResposta ParaResposta(this Tarefa tarefa)
    {
        return new TarefaResposta
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Status = tarefa.Status,
            CriadoEm = tarefa.CriadoEm,
            AtualizadoEm = tarefa.AtualizadoEm
        };
    }
}
