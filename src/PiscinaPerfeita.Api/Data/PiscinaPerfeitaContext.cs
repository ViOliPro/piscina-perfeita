using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Data;

public partial class PiscinaPerfeitaContext : DbContext
{
    public PiscinaPerfeitaContext(DbContextOptions<PiscinaPerfeitaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analise> Analises { get; set; }
    public virtual DbSet<Estoque> Estoques { get; set; }
    public virtual DbSet<MovimentacaoEstoque> MovimentacoesEstoques { get; set; }
    public virtual DbSet<Piscina> Piscinas { get; set; }
    public virtual DbSet<Produto> Produtos { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Vital para o PostgreSQL: Mantém os enums e geradores de UUID ativos
        modelBuilder
            .HasPostgresEnum("piscina-perfeita", "tipo_revestimento_enum", new[] { "Vinil", "Alvenaria", "Fibra de vidro", "Pastilhas", "Porcelanato/Ceramica", "Pedras naturais" })
            .HasPostgresEnum("piscina-perfeita", "tipo_tratamento_enum", new[] { "Quimico tradicional", "Salinizacao", "Ozonio", "Luz ultravioleta" })
            .HasPostgresExtension("uuid-ossp");

        // Configurações Globais por Tabela que as Data Annotations não cobrem 100%
        modelBuilder.Entity<Analise>(entity =>
        {
            entity.ToTable("Analises", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
            entity.Property(e => e.DataAnalise).HasColumnType("timestamp with time zone"); // Corrigido para DateTimeOffset compatível
        });

        modelBuilder.Entity<Estoque>(entity =>
        {
            entity.ToTable("Estoque", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("MovimentacoesEstoque", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
            entity.Property(e => e.DataMovimentacao).HasColumnType("timestamp with time zone"); // Corrigido para DateTimeOffset compatível
        });

        modelBuilder.Entity<Piscina>(entity =>
        {
            entity.ToTable("Piscinas", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");

            // CORRIGIDO: Configuração explícita do relacionamento Um-para-Muitos corrigido
            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.Piscinas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade); // Se deletar o usuário, deleta as piscinas dele
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            // Note que a sua tabela de usuário estava mapeada em minúsculo "usuarios" sem o schema! 
            // Mantive igual ao seu banco original para não perder dados.
            entity.ToTable("usuarios");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(256).HasColumnName("email");
            entity.Property(e => e.Nome).HasMaxLength(150).HasColumnName("nome");
            entity.Property(e => e.Senhahash).HasMaxLength(255).HasColumnName("senhahash");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
