using ApiV1ControlleurMonstre.Data.Context;
using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Models;
using ApiV1ControlleurMonstre.Utilities;


namespace ApiV1ControlleurMonstre.Services
{
    public class MonstreMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;

        public MonstreMaintenanceService(IServiceProvider serviceProvider, ILogger<MonstreMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private async Task ValidateMonsterCount(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            var monsterCount = await context.InstanceMonstres.CountAsync(cancellationToken);
            var monstersToCreate = 300 - monsterCount;

            if (monstersToCreate > 0)
            {
                _logger.LogInformation("Creating {Count} new monsters", monstersToCreate);
                await CreateMonsters(context, monstersToCreate, cancellationToken);
            }
        }

        private async Task CreateMonsters(MonsterContext context, int count, CancellationToken cancellationToken)
        {
            var random = new Random();
            var allMonstres = await context.Monstre.ToListAsync(cancellationToken);
            
            if (allMonstres == null || allMonstres.Count == 0)
            {
                _logger.LogError("No monster templates found in database. Cannot create monster instances.");
                return;
            }

            var villes = await context.Tuiles
                .Where(t => t.Type == TuileTypeEnum.Ville)
                .ToListAsync(cancellationToken);

            for (int i = 0; i < count; i++)
            {
                var validTile = await FindValidTileForMonster(context, random, cancellationToken);
                if (validTile == null) continue;

                var selectedMonstre = allMonstres[random.Next(allMonstres.Count)];
                var level = CalculateMonsterLevel(validTile, villes);

                var instanceMonstre = new InstanceMonstre
                {
                    MonstreId = selectedMonstre.Id,
                    PositionX = validTile.PositionX,
                    PositionY = validTile.PositionY,
                    Niveau = level,
                    PointsVieActuels = selectedMonstre.PointsVieBase + level,
                    PointsVieMax = selectedMonstre.PointsVieBase + level,
                };

                context.InstanceMonstres.Add(instanceMonstre);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Tuile> FindValidTileForMonster(MonsterContext context, Random random, CancellationToken cancellationToken)
        {
            // Essayer jusqu'à 10 fois de trouver une tuile valide
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = random.Next(0, 51); 
                int y = random.Next(0, 51);

                var tile = await context.Tuiles.FindAsync(new object[] { x, y }, cancellationToken);
                
                // Si la tuile n'existe pas, essayer de la générer jusqu'à avoir une tuile traversable
                if (tile == null)
                {
                    // Essayer jusqu'à 10 fois de générer une tuile traversable à cette position
                    for (int genAttempt = 0; genAttempt < 10; genAttempt++)
                    {
                        tile = TileGenerator.GenerateTuile(x, y);
                        
                        // Si la tuile générée est traversable et pas une ville, on la garde
                        if (tile.EstTraversable && tile.Type != TuileTypeEnum.Ville)
                        {
                            context.Tuiles.Add(tile);
                            await context.SaveChangesAsync(cancellationToken);
                            break;
                        }
                    }
                    
                    // Si après 10 tentatives on n'a pas de tuile valide, forcer une tuile d'herbe
                    if (tile == null || !tile.EstTraversable || tile.Type == TuileTypeEnum.Ville)
                    {
                        tile = new Tuile(x, y, TuileTypeEnum.Herbe, true, Tuile.stringImageUrl[TuileTypeEnum.Herbe]);
                        context.Tuiles.Add(tile);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }

                // Vérifier si la tuile est valide pour un monstre
                if (tile.EstTraversable && 
                    tile.Type != TuileTypeEnum.Ville && 
                    !await context.InstanceMonstres.AnyAsync(m => m.PositionX == x && m.PositionY == y, cancellationToken))
                {
                    return tile;
                }
            }

            return null;
        }

        private int CalculateMonsterLevel(Tuile monsterTile, List<Tuile> villes)
        {
            // Calculer la distance minimale à une ville
            double minDistance = double.MaxValue;
            foreach (var ville in villes)
            {
                double distance = Math.Sqrt(
                    Math.Pow(monsterTile.PositionX - ville.PositionX, 2) +
                    Math.Pow(monsterTile.PositionY - ville.PositionY, 2)
                );
                minDistance = Math.Min(minDistance, distance);
            }

            // Convertir la distance en niveau (1-10)
            // Plus la distance est grande, plus le niveau est élevé
            int level = (int)Math.Min(10, Math.Max(1, Math.Ceiling(minDistance / 10)));
            return level;
        }

        //ConÃ§u pour s'exÃ©cuter une seule fois et contenir une boucle.
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Check every 30 minutes
                                                                
            await ValidateMonsterCount(cancellationToken);

            // Continue checking periodically
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, cancellationToken);
                    await ValidateMonsterCount(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic monster count validation");
                    // Continue the loop - don't let one failure kill the service
                }
            }
        }
    }
}
