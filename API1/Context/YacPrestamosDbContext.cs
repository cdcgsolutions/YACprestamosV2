using API1.Models;
using Microsoft.EntityFrameworkCore;

namespace API1.Context;

public class YacPrestamosDbContext : DbContext
{
    public YacPrestamosDbContext(DbContextOptions<YacPrestamosDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Si se elimina un préstamo, se eliminan sus pagos en cascada
        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Prestamo)
            .WithMany(pr => pr.Pagos)
            .HasForeignKey(p => p.IdPrestamo)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Cliente -> Préstamos (Restringir borrado si tiene préstamos activos)
        modelBuilder.Entity<Prestamo>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Prestamos)
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Usuario (Auditoría) -> Préstamos
        modelBuilder.Entity<Prestamo>()
            .HasOne(p => p.UsuarioRegistro)
            .WithMany(u => u.PrestamosRegistrados)
            .HasForeignKey(p => p.IdUsuarioRegistro)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
