using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.DTOs
{
    public class TuileDto
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string TypeTuile { get; set; }
        public string ImageURL { get; set; }
        
        // Monstre présent sur la tuile (null si aucun)
        public InstanceMonstreDto? Monstre { get; set; }
        
        // Indique si le joueur peut se déplacer sur cette tuile
        public bool EstTraversable { get; set; }

        public static TuileDto FromModel(Tuile tuile, InstanceMonstre? instanceMonstre = null)
        {
            return new TuileDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                TypeTuile = tuile.Type.ToString(),
                ImageURL = tuile.ImageURL,
                EstTraversable = tuile.EstTraversable,
                Monstre = instanceMonstre != null 
                    ? InstanceMonstreDto.FromModel(instanceMonstre) 
                    : null
            };
        }
    }
}