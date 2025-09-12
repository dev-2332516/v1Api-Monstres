namespace ApiV1ControlleurMonstre.Models
{
    public class Personnage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie {  get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense  { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int UtilisateurID { get; set; }
        public DateTime DateCreation { get; set; }

        public Personnage() { }

        public Personnage(int id, string name, int niveau, int experience, int pointsVie, int pointsVieMax, int force, int defense, int positionX, int positionY, int utilisateurID, DateTime dateCreation)
        {
            Id = id;
            Name = name;
            Niveau = niveau;
            Experience = experience;
            PointsVie = pointsVie;
            PointsVieMax = pointsVieMax;
            Force = force;
            Defense = defense;
            PositionX = positionX;
            PositionY = positionY;
            UtilisateurID = utilisateurID;
            DateCreation = DateTime.Now;
        }
    }
}
