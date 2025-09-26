using Microsoft.EntityFrameworkCore;

namespace ApiV1ControlleurMonstre.Models
{
    [PrimaryKey(nameof(PositionX), nameof(PositionY))]
    public class InstanceMonstre
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MonstreId { get; set; }
        public int Niveau { get; set; }
        public int PointsVieMax { get; set; }
        public int PointsVieActuels { get; set; }
        public InstanceMonstre() { }

        public InstanceMonstre(int positionX, int positionY, int monstreId, int niveau, int pointsVieMax, int pointsVieActuel)
        {
            PositionX = positionX;
            PositionY = positionY;
            MonstreId = monstreId;
            Niveau = niveau;
            PointsVieMax = pointsVieMax;
            PointsVieActuels = pointsVieActuel;
        }
    }
}
