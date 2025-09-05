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
        public int Type { get; set; }
        public bool EstTraversable { get; set; }
        public string ImageURL { get; set; }

        public Tuile() { }

        public Tuile(int positionX, int positionY, int type, bool estTraversable, string imageURL)
        {
            PositionX = positionX;
            PositionY = positionY;
            Type = type;
            EstTraversable = estTraversable;
            ImageURL = imageURL;
        }
    }
}
