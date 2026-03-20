namespace GerenciadorTarefas.Api.Dominio.Excecoes;

public sealed class RegraDeNegocioException(string mensagem) : Exception(mensagem);
