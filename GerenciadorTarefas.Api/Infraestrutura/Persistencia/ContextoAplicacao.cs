using GerenciadorTarefas.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Api.Infraestrutura.Persistencia;

public sealed class ContextoAplicacao(DbContextOptions<ContextoAplicacao> options) : DbContext(options)
{
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextoAplicacao).Assembly);
    }
}
