using System.ComponentModel.DataAnnotations;
using GerenciadorTarefas.Api.Dominio.Enumeradores;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Aplicacao.DTOs.Requisicoes;

public sealed class ConsultaTarefasRequisicao
{
    [FromQuery(Name = "pageNumber")]
    [Range(1, int.MaxValue, ErrorMessage = "pageNumber deve ser maior que zero.")]
    public int NumeroPagina { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    [Range(1, 100, ErrorMessage = "pageSize deve estar entre 1 e 100.")]
    public int TamanhoPagina { get; init; } = 10;

    [FromQuery(Name = "status")]
    public StatusTarefa? Status { get; init; }
}
