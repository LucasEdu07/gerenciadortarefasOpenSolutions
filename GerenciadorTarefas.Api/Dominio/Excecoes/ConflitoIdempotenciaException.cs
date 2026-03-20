namespace GerenciadorTarefas.Api.Dominio.Excecoes;

public sealed class ConflitoIdempotenciaException(string mensagem) : Exception(mensagem);
