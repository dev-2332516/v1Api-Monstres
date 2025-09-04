using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Monstre> Monstre { get; set; }

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }
    }
}
