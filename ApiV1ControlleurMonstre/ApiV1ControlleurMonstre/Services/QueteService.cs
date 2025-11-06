using ApiV1ControlleurMonstre.Constants;
using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using ApiV1ControlleurMonstre.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Services
{
    public class QueteService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueteService> _logger;

        public QueteService(IServiceProvider serviceProvider, ILogger<QueteService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private async Task RegenerateQuest(MonsterContext context, CancellationToken cancellationToken)
        {
            var allQuests = await context.Quetes.ToListAsync(cancellationToken);
            var allCharacters = await context.Personnages.ToListAsync(cancellationToken);
            if (allCharacters.Count == 0)
            {
                _logger.LogError("There are no characters in the game");
                return;
            }
            foreach (var character in allCharacters)
            {
                // Verifie si les personnages n'ont pas 3 quests
                if (allQuests.Where(quest => quest.Personnage == character).ToList().Count() < 3)
                {
                    // Sinon, généré une quest

                    int typeQuete = Random.Shared.Next(3);
                    switch (typeQuete)
                    {
                        // Destination
                        case 0:
                            // Get une tuile au hazard
                            List<Tuile> listTuilesValide = await context.Tuiles
                                .Where(tuile =>tuile.EstTraversable == true)
                                .ToListAsync(cancellationToken);

                            Tuile destination = listTuilesValide[Random.Shared.Next(listTuilesValide.Count())];

                            // Créee la quete
                            _logger.LogInformation($"Creating a new tile quest for {character.Name}");
                            Quete queteDestination = new Quete($"Rendez-vous sur la tuile {destination.PositionX}, {destination.PositionY}", destination, character);
                            context.Quetes.Add(queteDestination);
                            break;
                        // Monstre
                        case 1:
                            // Prendre un monstre au hazard 
                            Monstre monstre = context.Monstre.ElementAt(Random.Shared.Next(context.Monstre.Count()));
                            // Prendre le type
                            string type = Random.Shared.Next(2) == 1 ? monstre.Type1 : monstre.Type2 ?? monstre.Type1;

                            _logger.LogInformation($"Creating a new monster quest for {character.Name}");
                            Quete queteMonstre = new Quete($"Tuer des monstres de type {type}", type, Random.Shared.Next(11), character);
                            context.Quetes.Add(queteMonstre);
                            break;
                        case 2:
                            _logger.LogInformation($"Creating a new level quest for {character.Name}");
                            Quete queteNiveau = new Quete($"Augmentez au niveau {character.Niveau + 1}", character);
                            context.Quetes.Add(queteNiveau);
                            break;
                    }
                }
            }
           await context.SaveChangesAsync(cancellationToken);
        }

        private async Task VerifyCompletion(MonsterContext context, CancellationToken cancellationToken)
        {
            List<Quete> allQuests = new List<Quete>((IEnumerable<Quete>)await context.Quetes.ToListAsync(cancellationToken));

            if (allQuests.Count == 0)
            {
                _logger.LogError("There are no quests to delete");
                return;
            }

            // Retirer les quetes qui sont complété
            _logger.LogInformation($"Removing all completed quests");
            context.RemoveRange(allQuests.Select(item => item.EstCompleter));
           
            await context.SaveChangesAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Check every 30 minutes

            throw new NotImplementedException();
        }
    }
}
