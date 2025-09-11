using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ApiV1ControlleurMonstre.Models
{
    public enum TuileTypeEnum
    {
        Herbe,
        Eau,
        Montagne,
        Foret,
        Ville,
        Route
    }
    public class Tuile
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public TuileTypeEnum Type { get; set; }
        public bool EstTraversable { get; set; }
        public string ImageURL { get; set; }

        public Tuile() { }

        public Tuile(int positionX, int positionY, TuileTypeEnum type, bool estTraversable, string imageURL)
        {
            PositionX = positionX;
            PositionY = positionY;
            Type = type;
            EstTraversable = estTraversable;
            ImageURL = imageURL;
        }

        public static Dictionary<TuileTypeEnum, string> stringImageUrl = new Dictionary<TuileTypeEnum, string>
        {
            {TuileTypeEnum.Herbe, "Plains.png" },
            {TuileTypeEnum.Eau, "River.png" },
            {TuileTypeEnum.Montagne, "Mountain.png" },
            {TuileTypeEnum.Foret, "Forest.png" },
            {TuileTypeEnum.Ville, "Town.png" },
            {TuileTypeEnum.Route, "Road.png" }
        };
    }
}
