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
        public Personnage Personnage { get; set; }
        public string Titre { get; set; } = string.Empty;
        public TypeQuete TypeQuete { get; set; }
        public bool EstCompleter { get; set; } = false;

        // Données pour destination
        public Tuile? Destination { get; set; } = null;

        // Données pour monstre
        public string TypeMonstre { get; set; } = string.Empty;
        public int NombreATuer { get; set; }
        public int NombreActuellementTuer { get; set; }

        // Niveau quand quest créé
        public int NiveauSauvegarder { get; set; }
        // Construction
        public Quete() { }

        public Quete(string titre, Tuile destination, Personnage personnage)
        {
            Titre = titre;
            Destination = destination;
            TypeQuete = TypeQuete.Destination;
            Personnage = personnage;
        }

        public Quete(string titre, string typeMonstre, int nombreATuer, Personnage personnage)
        {
            Titre = titre;
            TypeMonstre = typeMonstre;
            NombreATuer = nombreATuer;
            Personnage = personnage;
            TypeQuete = TypeQuete.Monstre;
        }

        public Quete(string titre, Personnage personnage)
        {
            Titre = titre;
            Personnage = personnage;
            NiveauSauvegarder = personnage.Niveau;
            TypeQuete = TypeQuete.Niveau;
        } 
    }
}
