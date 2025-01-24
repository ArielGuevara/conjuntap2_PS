using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace GestionInventario
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la entidad Producto
            modelBuilder.Entity<Producto>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Descripcion)
                .HasMaxLength(500);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId);

            // Configuración de la entidad Categoria
            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Descripcion)
                .HasMaxLength(500);

            // Configuración de la entidad Venta
            modelBuilder.Entity<Venta>()
                .HasKey(v => v.Id);

            modelBuilder.Entity<Venta>()
                .Property(v => v.FechaVenta)
                .IsRequired();

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Producto)
                .WithMany()
                .HasForeignKey(v => v.ProductoId);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId);

            // Configuración de la entidad Cliente
            modelBuilder.Entity<Cliente>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Email)
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Telefono)
                .HasMaxLength(15);
        }
    }

    public class Producto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        [Required]
        public decimal Precio { get; set; }
        public int CantidadStock { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }

    public class Categoria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }

    public class Venta
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public DateTime FechaVenta { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
    }

    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Nombre { get; set; }
        [Required]
        public string? Apellido { get; set; }
        [Required]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}
