using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.DTOs
{
    public class GrilleJeuDto
    {
        public List<TuileAvecInfosDto> Tuiles { get; set; }
        
        // Position centrale de la grille (position du joueur)
        public int CentreX { get; set; }
        public int CentreY { get; set; }
        
        // Informations du personnage
        public PersonnageDto Personnage { get; set; }

        public GrilleJeuDto()
        {
            Tuiles = new List<TuileAvecInfosDto>();
        }

        public static GrilleJeuDto FromModel(Personnage personnage, List<Tuile> tuiles, 
            Dictionary<(int, int), InstanceMonstre> monstres)
        {
            var dto = new GrilleJeuDto
            {
                CentreX = personnage.PositionX,
                CentreY = personnage.PositionY,
                Personnage = PersonnageDto.FromModel(personnage)
            };

            foreach (var tuile in tuiles)
            {
                var monstreSurTuile = monstres.ContainsKey((tuile.PositionX, tuile.PositionY))
                    ? monstres[(tuile.PositionX, tuile.PositionY)]
                    : null;

                dto.Tuiles.Add(TuileAvecInfosDto.FromModel(tuile, monstreSurTuile));
            }

            return dto;
        }
    }
}