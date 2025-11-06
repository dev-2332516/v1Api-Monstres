using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Tuile> Tuiles { get; set; }
        public DbSet<Personnage> Personnages { get; set; }
        public DbSet<Monstre> Monstre { get; set; }
        public DbSet<InstanceMonstre> InstanceMonstres { get; set; }


        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Utilisateur>().ToTable("utilisateur");
            modelBuilder.Entity<Tuile>().HasKey(t => new { t.PositionX, t.PositionY });
            modelBuilder.Entity<InstanceMonstre>()
                .HasOne(im => im.Monstre)
                .WithMany()
                .HasForeignKey(im => im.MonstreId)
                .OnDelete(DeleteBehavior.Restrict);
   
            base.OnModelCreating(modelBuilder);
        }
    }
}
