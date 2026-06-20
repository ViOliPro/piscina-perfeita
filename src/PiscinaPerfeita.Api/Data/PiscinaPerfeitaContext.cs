using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Models;

namespace PiscinaPerfeita.Api.Data;

public partial class PiscinaPerfeitaContext : DbContext
{
    public PiscinaPerfeitaContext()
    {
    }

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

    public virtual DbSet<Usuario1> Usuarios1 { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("piscina-perfeita", "tipo_revestimento_enum", new[] { "Vinil", "Alvenaria", "Fibra de vidro", "Pastilhas", "Porcelanato/Ceramica", "Pedras naturais" })
            .HasPostgresEnum("piscina-perfeita", "tipo_tratamento_enum", new[] { "Quimico tradicional", "Salinizacao", "Ozonio", "Luz ultravioleta" })
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Analise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Analises_pkey");

            entity.ToTable("Analises", "piscina-perfeita");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DataAnalise).HasColumnType("time with time zone");
            entity.Property(e => e.Observacoes).HasColumnType("char");

            entity.HasOne(d => d.Piscina).WithMany(p => p.Analises)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PiscinaId");
        });

        modelBuilder.Entity<Estoque>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Estoque_pkey");

            entity.ToTable("Estoque", "piscina-perfeita");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Piscina).WithMany(p => p.Estoques)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PiscinaId");

            entity.HasOne(d => d.Produto).WithMany(p => p.Estoques)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProdutoId");
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MovimentacoesEstoque_pkey");

            entity.ToTable("MovimentacoesEstoque", "piscina-perfeita");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.DataMovimentacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("time with time zone");
            entity.Property(e => e.TipoMovimentacao).HasColumnType("char");

            entity.HasOne(d => d.Piscina).WithMany(p => p.MovimentacoesEstoques)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PiscinaId");

            entity.HasOne(d => d.Produto).WithMany(p => p.MovimentacoesEstoques)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProdutoId");
        });

        modelBuilder.Entity<Piscina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Piscinas_pkey");

            entity.ToTable("Piscinas", "piscina-perfeita");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Nome).HasMaxLength(100);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Piscinas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsuarioId");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Produtos_pkey");

            entity.ToTable("Produtos", "piscina-perfeita");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Nome).HasColumnType("char");
            entity.Property(e => e.UnidadeMedida).HasColumnType("char");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(150)
                .HasColumnName("nome");
            entity.Property(e => e.Senhahash)
                .HasMaxLength(255)
                .HasColumnName("senhahash");
        });

        modelBuilder.Entity<Usuario1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Usuarios_pkey");

            entity.ToTable("Usuarios", "piscina-perfeita");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.SenhaHash).HasMaxLength(100);

            entity.HasOne(d => d.Piscina).WithMany(p => p.Usuario1s)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PiscinaId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
