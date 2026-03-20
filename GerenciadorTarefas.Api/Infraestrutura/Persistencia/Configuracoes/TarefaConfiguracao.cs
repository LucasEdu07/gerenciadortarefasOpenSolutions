using GerenciadorTarefas.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Api.Infraestrutura.Persistencia.Configuracoes;

public sealed class TarefaConfiguracao : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("Tarefas");

        builder.HasKey(tarefa => tarefa.Id);

        builder.Property(tarefa => tarefa.Titulo)
            .IsRequired();

        builder.Property(tarefa => tarefa.Descricao);

        builder.Property(tarefa => tarefa.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(tarefa => tarefa.CriadoEm)
            .IsRequired();

        builder.Property(tarefa => tarefa.AtualizadoEm)
            .IsRequired();
    }
}
