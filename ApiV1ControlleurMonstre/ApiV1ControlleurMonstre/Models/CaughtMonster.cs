namespace ApiV1ControlleurMonstre.Models
{
    public class CaughtMonster
    {
        public int Id { get; set; }
        public Monstre monstreCaught { get; set; }
        public Personnage whoHasCaught { get; set; }

        public CaughtMonster() { }
    }
}
