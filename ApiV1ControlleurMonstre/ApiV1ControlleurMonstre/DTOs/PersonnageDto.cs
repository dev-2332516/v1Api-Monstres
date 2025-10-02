using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.DTOs
{
    public class PersonnageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Niveau { get; set; }
        public int Experience { get; set; }
        public int PointsVie { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public static PersonnageDto FromModel(Personnage personnage)
        {
            return new PersonnageDto
            {
                Id = personnage.Id,
                Name = personnage.Name,
                Niveau = personnage.Niveau,
                Experience = personnage.Experience,
                PointsVie = personnage.PointsVie,
                PointsVieMax = personnage.PointsVieMax,
                Force = personnage.Force,
                Defense = personnage.Defense,
                PositionX = personnage.PositionX,
                PositionY = personnage.PositionY
            };
        }
    }
}