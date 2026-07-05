using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Data;

public partial class PiscinaPerfeitaContext : DbContext
{
    private readonly IAuthenticatedUser authenticatedUser;

    //O EF core injenta o Helper IAuthenticatedUser automaticamente a cada requisição.
    public PiscinaPerfeitaContext(
        DbContextOptions<PiscinaPerfeitaContext> options,
        IAuthenticatedUser authenticatedUser
    )
        : base(options)
    {
        this.authenticatedUser = authenticatedUser;
    }

    // Propriedade para acessar o LocalId do usuário autenticado
    private Guid CurrentLocalId => authenticatedUser.GetLocalId();

    public virtual DbSet<Analise> Analises { get; set; }
    public virtual DbSet<Estoque> Estoques { get; set; }
    public virtual DbSet<MovimentacaoEstoque> MovimentacoesEstoques { get; set; }
    public virtual DbSet<Piscina> Piscinas { get; set; }
    public virtual DbSet<Produto> Produtos { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Local> Locais { get; set; }
    public virtual DbSet<UsuarioLocal> UsuariosLocal { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica o filtro de LocalId automaticamente para TODAS as entidades que herdam de IBelongsToLocal
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IBelongsToLocal).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                //e.LocalId
                var property = Expression.Property(parameter, nameof(IBelongsToLocal.LocalId));

                //o valor da classe: This.CurrentLocalId
                var dbContectInstance = Expression.Constant(this);
                var localIdProperty = Expression.Property(
                    dbContectInstance,
                    nameof(CurrentLocalId)
                );

                // Expressao final: e => e.LocalId == this.CurrentLocalId
                var comparison = Expression.Equal(property, localIdProperty);
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // Vital para o PostgreSQL: Mantém os enums e geradores de UUID ativos
        modelBuilder
            .HasPostgresEnum(
                "piscina-perfeita",
                "tipo_revestimento_enum",
                new[]
                {
                    "Vinil",
                    "Alvenaria",
                    "Fibra de vidro",
                    "Pastilhas",
                    "Porcelanato/Ceramica",
                    "Pedras naturais",
                }
            )
            .HasPostgresEnum(
                "piscina-perfeita",
                "tipo_tratamento_enum",
                new[] { "Quimico tradicional", "Salinizacao", "Ozonio", "Luz ultravioleta" }
            )
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
            entity.ToTable("Estoques", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("Movimentacoes", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
            entity.Property(e => e.DataMovimentacao).HasColumnType("timestamp with time zone"); // Corrigido para DateTimeOffset compatível
        });

        modelBuilder.Entity<Piscina>(entity =>
        {
            entity.ToTable("Piscinas", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");

            // CORRIGIDO: Configuração explícita do relacionamento Um-para-Muitos corrigido
            entity
                .HasOne(d => d.Usuario)
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
            entity.ToTable("Usuarios", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(256).HasColumnName("email");
            entity.Property(e => e.Nome).HasMaxLength(150).HasColumnName("nome");
            entity.Property(e => e.SenhaHash).HasMaxLength(255).HasColumnName("password");
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
        });

        modelBuilder.Entity<Local>(entity =>
        {
            entity.ToTable("Locais", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
        });

        modelBuilder.Entity<UsuarioLocal>(entity =>
        {
            entity.ToTable("UsuariosLocal", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()"); // Mudado para gerar automático
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    // Automação completa para o fluxo de inserção (Create)
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IBelongsToLocal && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            // Atribui o LocalId automaticamente antes de salvar no banco
            ((IBelongsToLocal)entry.Entity).LocalId = authenticatedUser.GetLocalId();
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
