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
        }
}