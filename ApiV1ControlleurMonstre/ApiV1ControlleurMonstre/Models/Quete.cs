namespace ApiV1ControlleurMonstre.Models
{
    public enum TypeQuete
    {
        Destination,
        Monstre,
        Niveau
    }
    public class Quete
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public TypeQuete TypeQuete { get; set; }

        // Données pour destination
        public Tuile? Destination { get; set; } = null;

        // Données pour monstre
        public string TypeMonstre { get; set; } = string.Empty;
        public int NombreATuer { get; set; }
        public int NombreActuellementTuer { get; set; }

        // Construction
        public Quete() { }

        public Quete(string titre, Tuile destination)
        {
            Titre = titre;
            Destination = destination;
            TypeQuete = TypeQuete.Destination;
        }

        public Quete(string titre, string typeMonstre, int nombreATuer)
        {
            Titre = titre;
            TypeMonstre = typeMonstre;
            NombreATuer = nombreATuer;
            TypeQuete = TypeQuete.Monstre;
        }

        public Quete(string titre)
        {
            Titre = titre;
            TypeQuete = TypeQuete.Niveau;
        } 
    }
}
