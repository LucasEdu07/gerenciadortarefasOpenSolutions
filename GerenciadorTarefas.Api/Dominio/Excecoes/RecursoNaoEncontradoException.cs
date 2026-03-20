namespace GerenciadorTarefas.Api.Dominio.Excecoes;

public sealed class RecursoNaoEncontradoException(string mensagem) : Exception(mensagem);
