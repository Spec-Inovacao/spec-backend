using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace YourNamespace.Data
{
    public class YourDbContext : DbContext
    {
        public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

        public DbSet<Servico> pservicos { get; set; }
        public DbSet<Agendamento> pagendamentos { get; set; }
        public DbSet<Usuario> pusuarios { get; set; }
        public DbSet<Bloqueio> pbloqagenda { get; set; }
        public DbSet<Configuracao> pconfigexp { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar o relacionamento entre Agendamento e Servico
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Servico)
                .WithMany()
                .HasForeignKey(a => a.servicoid)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}