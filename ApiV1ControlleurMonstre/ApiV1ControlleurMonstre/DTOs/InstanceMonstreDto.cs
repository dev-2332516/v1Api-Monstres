using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.DTOs
{
    public class InstanceMonstreDto
    {
        public int MonstreId { get; set; }
        public string Nom { get; set; }
        public string SpriteURL { get; set; }
        public int Niveau { get; set; }
        
        // Position sur la carte
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        
        // Statistiques calculées du monstre
        public int PointsVieActuels { get; set; }
        public int PointsVieMax { get; set; }
        public int Force { get; set; }
        public int Defense { get; set; }
        public int Experience { get; set; }
        
        // Type du monstre
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        
        public static InstanceMonstreDto FromModel(InstanceMonstre instance)
        {
            // Nous supposons que le Monstre est déjà chargé grâce à Include
            return new InstanceMonstreDto
            {
                MonstreId = instance.MonstreId,
                Nom = instance.Monstre.Nom,
                SpriteURL = instance.Monstre.SpriteURL,
                Niveau = instance.Niveau,
                PositionX = instance.PositionX,
                PositionY = instance.PositionY,
                PointsVieActuels = instance.PointsVieActuels,
                PointsVieMax = instance.PointsVieMax,
                Force = instance.Monstre.ForceBase + instance.Niveau,
                Defense = instance.Monstre.DefenseBase + instance.Niveau,
                Experience = instance.Monstre.ExperienceBase * instance.Niveau,
                Type1 = instance.Monstre.Type1,
                Type2 = instance.Monstre.Type2
            };
        }
    }
}