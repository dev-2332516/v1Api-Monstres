using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre
{
    public static class GenTuile
    {
        public static Tuile GenerateTuile(int positionX, int positionY)
        {
            Tuile tuile;
            Random random = new Random();
            int randomNumber = random.Next(1, 101);
            TuileTypeEnum type;
            string imageUrl;
            if (randomNumber <= 20)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Herbe, true, Tuile.stringImageUrl[TuileTypeEnum.Herbe]);
            }
            else if (randomNumber <= 30)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Eau, false, Tuile.stringImageUrl[TuileTypeEnum.Eau]);
            }
            else if (randomNumber <= 45)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Montagne, false, Tuile.stringImageUrl[TuileTypeEnum.Montagne]);
            }
            else if (randomNumber <= 60)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Foret, true, Tuile.stringImageUrl[TuileTypeEnum.Foret]);
            }
            else if (randomNumber <= 65)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Ville, true, Tuile.stringImageUrl[TuileTypeEnum.Ville]);
            }
            else
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Route, true, Tuile.stringImageUrl[TuileTypeEnum.Route]);
            }
            return tuile;
        }
    }
}
