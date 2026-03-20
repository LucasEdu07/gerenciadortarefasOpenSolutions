using GerenciadorTarefas.Api.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Api.Infraestrutura.Persistencia.Configuracoes;

public sealed class RegistroIdempotenciaConfiguracao : IEntityTypeConfiguration<RegistroIdempotencia>
{
    public void Configure(EntityTypeBuilder<RegistroIdempotencia> builder)
    {
        builder.ToTable("RegistrosIdempotencia");

        builder.HasKey(registro => registro.Chave);

        builder.Property(registro => registro.Chave)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(registro => registro.HashRequisicao)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(registro => registro.TarefaId)
            .IsRequired();

        builder.Property(registro => registro.RespostaEmJson)
            .IsRequired();

        builder.Property(registro => registro.CriadoEm)
            .IsRequired();

        builder.HasIndex(registro => registro.TarefaId);
    }
}
