using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.Utilities
{
    /// <summary>
    /// Générateur de tuiles procédurales pour le monde du jeu
    /// </summary>
    public static class TileGenerator
    {
        /// <summary>
        /// Génère une tuile aléatoire à la position spécifiée
        /// </summary>
        /// <param name="positionX">Position X de la tuile</param>
        /// <param name="positionY">Position Y de la tuile</param>
        /// <returns>Une nouvelle tuile générée</returns>
        public static Tuile GenerateTuile(int positionX, int positionY)
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 101);
            
            // Distribution des types de tuiles:
            // 1-20%: Herbe (traversable)
            // 21-30%: Eau (non-traversable) 
            // 31-45%: Montagne (non-traversable)
            // 46-60%: Forêt (traversable)
            // 61-65%: Ville (traversable)
            // 66-100%: Route (traversable)
            
            if (randomNumber <= 20)
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Herbe, true, Tuile.stringImageUrl[TuileTypeEnum.Herbe]);
            }
            else if (randomNumber <= 30)
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Eau, false, Tuile.stringImageUrl[TuileTypeEnum.Eau]);
            }
            else if (randomNumber <= 45)
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Montagne, false, Tuile.stringImageUrl[TuileTypeEnum.Montagne]);
            }
            else if (randomNumber <= 60)
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Foret, true, Tuile.stringImageUrl[TuileTypeEnum.Foret]);
            }
            else if (randomNumber <= 65)
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Ville, true, Tuile.stringImageUrl[TuileTypeEnum.Ville]);
            }
            else
            {
                return new Tuile(positionX, positionY, TuileTypeEnum.Route, true, Tuile.stringImageUrl[TuileTypeEnum.Route]);
            }
        }
    }
}