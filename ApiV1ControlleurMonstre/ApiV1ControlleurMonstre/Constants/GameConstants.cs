namespace ApiV1ControlleurMonstre.Constants
{
    /// <summary>
    /// Constantes du jeu pour éviter les nombres magiques
    /// </summary>
    public static class GameConstants
    {
        // Limites de la carte
        public const int MAP_MIN_POSITION = 0;
        public const int MAP_MAX_POSITION = 50;
        
        // Système d'expérience
        public const int EXPERIENCE_PER_LEVEL = 100;
        public const int BASE_MONSTER_XP_MULTIPLIER = 10;
        
        // Stats de base du personnage
        public const int DEFAULT_PLAYER_HP = 100;
        public const int DEFAULT_PLAYER_MAX_HP = 999;
        public const int DEFAULT_PLAYER_FORCE = 10;
        public const int DEFAULT_PLAYER_DEFENSE = 10;
        public const int DEFAULT_PLAYER_LEVEL = 0;
        
        // Combat
        public const double MIN_COMBAT_FACTOR = 0.8;
        public const double MAX_COMBAT_FACTOR = 1.25;
        public const int MIN_DAMAGE = 1;
        
        // Spawn du personnage
        public const int MIN_SPAWN_POSITION = MAP_MIN_POSITION + 2;
        public const int MAX_SPAWN_POSITION = MAP_MAX_POSITION - 2;
        
        // JWT Token
        public const int JWT_EXPIRY_HOURS = 1;
    }
}