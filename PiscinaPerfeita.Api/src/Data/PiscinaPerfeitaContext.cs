using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Models.Interfaces;

namespace PiscinaPerfeita.Api.Data;

public partial class PiscinaPerfeitaContext : DbContext
{
    private readonly IAuthenticatedUser authenticatedUser;

    public PiscinaPerfeitaContext(
        DbContextOptions<PiscinaPerfeitaContext> options,
        IAuthenticatedUser authenticatedUser
    )
        : base(options)
    {
        this.authenticatedUser = authenticatedUser;
    }

    private Guid CurrentLocalId => authenticatedUser.GetLocalId();

    private bool IsSuperAdminUser => authenticatedUser.IsSuperAdmin();

    public virtual DbSet<Analise> Analises { get; set; }
    public virtual DbSet<Estoque> Estoques { get; set; }
    public virtual DbSet<MovimentacaoEstoque> MovimentacoesEstoques { get; set; }
    public virtual DbSet<Piscina> Piscinas { get; set; }
    public virtual DbSet<Produto> Produtos { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Local> Locais { get; set; }
    public virtual DbSet<UsuarioLocal> UsuariosLocal { get; set; }
    public virtual DbSet<Deposito> Depositos { get; set; }
    public virtual DbSet<AplicacaoProduto> AplicacoesProduto { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<ConviteToken> ConviteTokens { get; set; }
    public virtual DbSet<Hidrometro> Hidrometros { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IBelongsToLocal).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IBelongsToLocal.LocalId));
                var dbContectInstance = Expression.Constant(this);
                var localIdProperty = Expression.Property(
                    dbContectInstance,
                    nameof(CurrentLocalId)
                );
                var isSuperAdminProperty = Expression.Property(
                    dbContectInstance,
                    nameof(IsSuperAdminUser)
                );
                var currentLocalIdEhVazio = Expression.Equal(
                    localIdProperty,
                    Expression.Constant(Guid.Empty)
                );
                var superAdminEmModoVerTodos = Expression.AndAlso(
                    isSuperAdminProperty,
                    currentLocalIdEhVazio
                );
                var comparison = Expression.Equal(property, localIdProperty);
                var comparisonOuSuperAdmin = Expression.OrElse(
                    comparison,
                    superAdminEmModoVerTodos
                );
                var lambda = Expression.Lambda(comparisonOuSuperAdmin, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

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

        modelBuilder.Entity<Analise>(entity =>
        {
            entity.ToTable("Analises", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DataAnalise).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Estoque>(entity =>
        {
            entity.ToTable("Estoques", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("Movimentacoes", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DataMovimentacao).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Piscina>(entity =>
        {
            entity.ToTable("Piscinas", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
            entity
                .HasOne(d => d.Usuario)
                .WithMany(p => p.Piscinas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(256).HasColumnName("email");
            entity.Property(e => e.Nome).HasMaxLength(150).HasColumnName("nome");
            entity.Property(e => e.SenhaHash).HasMaxLength(255).HasColumnName("password");
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Local>(entity =>
        {
            entity.ToTable("Locais", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
        });

        modelBuilder.Entity<UsuarioLocal>(entity =>
        {
            entity.ToTable("UsuariosLocal", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("createtat");
        });

        modelBuilder.Entity<Deposito>(entity =>
        {
            entity.ToTable("Depositos", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<AplicacaoProduto>(entity =>
        {
            entity.ToTable("AplicacoesProduto", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DataAplicacao).HasColumnType("timestamp with time zone");
            entity
                .HasOne(a => a.Analise)
                .WithMany()
                .HasForeignKey(a => a.AnaliseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TokenHash).IsRequired();
            entity.Property(e => e.ExpiraEm).IsRequired();
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(e => e.UsadoEm).IsRequired(false);
            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity
                .HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConviteToken>(entity =>
        {
            entity.ToTable("ConviteTokens", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.TokenHash).IsRequired();
            entity.Property(e => e.ExpiraEm).IsRequired();
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(e => e.UsadoEm).IsRequired(false);
            entity.HasIndex(t => t.TokenHash).IsUnique();
        });

        modelBuilder.Entity<Hidrometro>(entity =>
        {
            entity.ToTable("Hidrometro", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity
                .Property(e => e.LeituraAtual)
                .HasColumnName("leituraatual")
                .HasPrecision(18, 2)
                .IsRequired();
            entity
                .Property(e => e.DataLeitura)
                .HasColumnName("dataleitura")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            entity.Property(e => e.Observacoes).HasColumnName("observacoes").HasMaxLength(500);
            entity.HasIndex(e => new { e.LocalId, e.DataLeitura }).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken", "piscina-perfeita");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity
                .Property(e => e.TokenHash)
                .HasColumnName("tokenhash")
                .IsRequired()
                .HasMaxLength(256);
            entity
                .Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();
            entity.Property(e => e.ExpiraEm).HasColumnName("expira_em").IsRequired();
            entity.Property(e => e.RevogadoEm).HasColumnName("revogado_em").IsRequired(false);
            entity
                .HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.UsuarioId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "piscina-perfeita");
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity
                .Property(e => e.OccurredAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
            entity.HasIndex(e => new { e.LocalId, e.OccurredAt });
            entity.HasIndex(e => new { e.UsuarioId, e.OccurredAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.OccurredAt });
            entity.HasIndex(e => new { e.Action, e.OccurredAt });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IBelongsToLocal && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            var localId = authenticatedUser.GetLocalId();
            if (localId == Guid.Empty)
                throw new InvalidOperationException(
                    "Selecione um Local ativo antes de criar este registro."
                );

            ((IBelongsToLocal)entry.Entity).LocalId = localId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
