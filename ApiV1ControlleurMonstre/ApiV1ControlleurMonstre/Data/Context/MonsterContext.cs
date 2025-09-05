using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Monstre> Monstre { get; set; }
        public DbSet<Tuile> Tuiles { get; set; }

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tuile>()
                .HasKey(t => new { t.PositionX, t.PositionY });

            base.OnModelCreating(modelBuilder);
        }
    }
}
