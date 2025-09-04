namespace ApiV1ControlleurMonstre.Models
{
    public class Monstre
    {
        public int Id { get; set; }
        public int PokemonId { get; set; }
        public string Nom {  get; set; }
        public int PointsVieBase { get; set; }
        public int ForceBase { get; set; }
        public int DefenseBase { get; set; }
        public int ExperienceBase { get; set; }
        public string SpriteURL { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }

        public Monstre() { }
        public Monstre(int id, int pokemonId, string nom, int pointsVieBase, int forceBase, int defenseBase, int experienceBase, string spriteURL, string type1, string type2)
        {
            Id = id;
            PokemonId = pokemonId;
            Nom = nom;
            PointsVieBase = pointsVieBase;
            ForceBase = forceBase;
            DefenseBase = defenseBase;
            ExperienceBase = experienceBase;
            SpriteURL = spriteURL;
            Type1 = type1;
            Type2 = type2;
        }
    }
}
