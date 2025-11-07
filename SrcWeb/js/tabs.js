// Interface simplifiée - Gestion des overlays et actions
document.addEventListener("DOMContentLoaded", () => {
  initializeInterface();
  updateCurrentQuest();
});

function initializeInterface() {
  // L'overlay est caché par défaut
  const overlay = document.getElementById("info-overlay");
  if (overlay) {
    overlay.classList.add("hidden");
  }
}

// Gestion de l'overlay d'informations
function hideInfoOverlay() {
  const overlay = document.getElementById("info-overlay");
  if (overlay) {
    overlay.classList.add("hidden");
  }
}

function showInfoOverlay(title, content) {
  const overlay = document.getElementById("info-overlay");
  const titleEl = document.getElementById("info-title");
  const bodyEl = document.getElementById("info-body");
  
  if (overlay && titleEl && bodyEl) {
    titleEl.textContent = title;
    bodyEl.innerHTML = content;
    overlay.classList.remove("hidden");
  }
}

// Actions des boutons de la barre d'action
async function showStats() {
  try {
    const personnage = await callAPI(`Personnages/GetPersonnageFromUser/`, "GET");
    const statsContent = `
      <div class="info-row">
        <span>❤️ Points de Vie:</span>
        <span>${personnage.pointsVie}/${personnage.pointsVieMax}</span>
      </div>
      <div class="info-row">
        <span>⭐ Niveau:</span>
        <span>${personnage.niveau}</span>
      </div>
      <div class="info-row">
        <span>💪 Force:</span>
        <span>${personnage.force}</span>
      </div>
      <div class="info-row">
        <span>🛡️ Défense:</span>
        <span>${personnage.defense}</span>
      </div>
      <div class="info-row">
        <span>⚡ Expérience:</span>
        <span>${personnage.experience}</span>
      </div>
      <div class="info-row">
        <span>📍 Position:</span>
        <span>${personnage.positionX}, ${personnage.positionY}</span>
      </div>
    `;
    showInfoOverlay("📊 Statistiques du Joueur", statsContent);
  } catch (error) {
    console.error("Erreur lors du chargement des statistiques:", error);
    showInfoOverlay("📊 Statistiques du Joueur", "<p>Erreur lors du chargement des statistiques</p>");
  }
}

function showQuests() {
  const questsContent = `
    <div style="margin-bottom: 1rem;">
      <div style="background: rgba(45, 45, 68, 0.6); padding: 0.8rem; border-radius: 6px; margin-bottom: 0.5rem;">
        <div style="color: #64b5f6; font-weight: bold; margin-bottom: 0.3rem;">🗺️ Explorez la forêt mystérieuse</div>
        <div style="font-size: 0.9rem; color: rgba(255, 255, 255, 0.8);">Se rendre aux coordonnées (25, 30)</div>
        <div style="font-size: 0.85rem; color: rgba(255, 255, 255, 0.6);">Progression: 0/1</div>
      </div>
      <div style="background: rgba(45, 45, 68, 0.6); padding: 0.8rem; border-radius: 6px; margin-bottom: 0.5rem;">
        <div style="color: #64b5f6; font-weight: bold; margin-bottom: 0.3rem;">⚔️ Chasseur de gobelins</div>
        <div style="font-size: 0.9rem; color: rgba(255, 255, 255, 0.8);">Éliminez 5 gobelins</div>
        <div style="font-size: 0.85rem; color: rgba(255, 255, 255, 0.6);">Progression: 2/5</div>
      </div>
      <div style="background: rgba(45, 45, 68, 0.6); padding: 0.8rem; border-radius: 6px;">
        <div style="color: #64b5f6; font-weight: bold; margin-bottom: 0.3rem;">📈 Apprenti aventurier</div>
        <div style="font-size: 0.9rem; color: rgba(255, 255, 255, 0.8);">Atteignez le niveau 5</div>
        <div style="font-size: 0.85rem; color: rgba(255, 255, 255, 0.6);">Progression: 3/5</div>
      </div>
    </div>
  `;
  showInfoOverlay("📋 Quêtes Actives", questsContent);
}

function showMap() {
  const coords = window.currentCoords || { x: 25, y: 25 };
  const mapContent = `
    <div class="info-row">
      <span>Position actuelle:</span>
      <span>${coords.x}, ${coords.y}</span>
    </div>
    <div class="info-row">
      <span>Taille du monde:</span>
      <span>50x50</span>
    </div>
    <div class="info-row">
      <span>Zone affichée:</span>
      <span>5x5</span>
    </div>
    <div class="info-row">
      <span>Status API:</span>
      <span>Connecté</span>
    </div>
    <hr style="margin: 0.8rem 0; border: none; height: 1px; background: rgba(100, 181, 246, 0.3);">
    <div style="margin-bottom: 0.5rem; color: #64b5f6; font-weight: bold;">Légende:</div>
    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.3rem;">
      <div style="width: 12px; height: 12px; background: linear-gradient(45deg, #333366, #404066); border-radius: 2px;"></div>
      <span style="font-size: 0.9rem;">Tuile standard</span>
    </div>
    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.3rem;">
      <div style="width: 12px; height: 12px; background: linear-gradient(45deg, #64b5f6, #5a9ed8); border-radius: 2px;"></div>
      <span style="font-size: 0.9rem;">Tuile chargée</span>
    </div>
    <div style="display: flex; align-items: center; gap: 0.5rem;">
      <div style="width: 12px; height: 12px; background: linear-gradient(45deg, #ffeb3b, #ffc107); border-radius: 2px;"></div>
      <span style="font-size: 0.9rem;">Tuile sélectionnée</span>
    </div>
  `;
  showInfoOverlay("🗺️ Informations Carte", mapContent);
}

// Met à jour la quête actuelle affichée
function updateCurrentQuest() {
  const currentQuestEl = document.getElementById("current-quest");
  if (currentQuestEl) {
    // Par défaut, affiche la première quête active
    // Dans un vrai jeu, cela viendrait d'un système de gestion des quêtes
    currentQuestEl.innerHTML = `
      <span class="quest-icon">⚔️</span>
      <span class="quest-text">Chasseur de gobelins (2/5)</span>
    `;
  }
}

// Fonction pour afficher les informations d'une tuile sélectionnée
function showTileInfo(x, y, type, traversable) {
  const tileContent = `
    <div class="info-row">
      <span>Coordonnées:</span>
      <span>${x}, ${y}</span>
    </div>
    <div class="info-row">
      <span>Type de tuile:</span>
      <span>${type || "Inconnue"}</span>
    </div>
    <div class="info-row">
      <span>Traversable:</span>
      <span>${traversable ? "Oui" : "Non"}</span>
    </div>
  `;
  showInfoOverlay("📍 Tuile Sélectionnée", tileContent);
}

// Fonction pour afficher les informations d'un monstre
function showMonsterInfo(monster) {
  const monsterContent = `
    <div class="info-row">
      <span>Nom:</span>
      <span>${monster.nom || "Inconnu"}</span>
    </div>
    <div class="info-row">
      <span>Points de Vie:</span>
      <span>${monster.pointsVieActuels || 0}/${monster.pointsVieMax || 0}</span>
    </div>
    <div class="info-row">
      <span>Force:</span>
      <span>${monster.force || 0}</span>
    </div>
    <div class="info-row">
      <span>Défense:</span>
      <span>${monster.defense || 0}</span>
    </div>
    <div class="info-row">
      <span>Niveau:</span>
      <span>${monster.niveau || 0}</span>
    </div>
    <div class="info-row">
      <span>Type #1:</span>
      <span>${monster.type1 || "-"}</span>
    </div>
    ${monster.type2 ? `
    <div class="info-row">
      <span>Type #2:</span>
      <span>${monster.type2}</span>
    </div>
    ` : ''}
  `;
  showInfoOverlay("👹 Informations Monstre", monsterContent);
}

// Exposer les fonctions pour les autres modules
window.gameInterface = {
  showTileInfo,
  showMonsterInfo,
  updateCurrentQuest,
  hideInfoOverlay
};

// Gestion des quêtes simplifiée pour la nouvelle interface
let currentQuests = [];

async function loadQuests() {
  try {
    // Quêtes de démonstration adaptées à l'interface mini
    currentQuests = [
      {
        id: 1,
        titre: "Explorez la forêt mystérieuse",
        typeQuete: "Destination",
        destination: { x: 25, y: 30 },
        progress: 0,
        maxProgress: 1,
        completed: false,
      },
      {
        id: 2,
        titre: "Chasseur de gobelins",
        typeQuete: "Monstre",
        typeMonstre: "Gobelin",
        nombreATuer: 5,
        nombreActuellementTuer: 2,
        progress: 2,
        maxProgress: 5,
        completed: false,
      },
      {
        id: 3,
        titre: "Apprenti aventurier",
        typeQuete: "Niveau",
        niveauCible: 5,
        progress: 3,
        maxProgress: 5,
        completed: false,
      },
    ];

    displayQuests();
  } catch (error) {
    console.error("Erreur lors du chargement des quêtes:", error);
    displayQuestsError();
  }
}

function displayQuests() {
  const questsList = document.getElementById("quests-list");
  if (!questsList) return;

  if (currentQuests.length === 0) {
    questsList.innerHTML = '<div class="quest-mini">Aucune quête active</div>';
    return;
  }

  questsList.innerHTML = currentQuests
    .filter(quest => !quest.completed)
    .map((quest) => createQuestMiniHTML(quest))
    .join("");
}

function createQuestMiniHTML(quest) {
  const icon = getQuestIcon(quest.typeQuete);
  const progressText = getQuestMiniProgressText(quest);
  
  return `<div class="quest-mini" data-quest-id="${quest.id}" title="${quest.titre}">
    ${icon} ${quest.titre.substring(0, 25)}${quest.titre.length > 25 ? '...' : ''} (${progressText})
  </div>`;
}

function getQuestIcon(typeQuete) {
  switch (typeQuete) {
    case "Destination": return "🗺️";
    case "Monstre": return "⚔️";
    case "Niveau": return "📈";
    default: return "❓";
  }
}

function getQuestMiniProgressText(quest) {
  switch (quest.typeQuete) {
    case "Destination":
      return quest.completed ? "✓" : `${quest.destination.x},${quest.destination.y}`;
    case "Monstre":
      return `${quest.nombreActuellementTuer}/${quest.nombreATuer}`;
    case "Niveau":
      return `${quest.progress}/${quest.maxProgress}`;
    default:
      return `${quest.progress}/${quest.maxProgress}`;
  }
}

function displayQuestsError() {
  const questsList = document.getElementById("quests-list");
  if (questsList) {
    questsList.innerHTML =
      '<p class="no-quests" style="color: #ff6b6b;">Erreur lors du chargement des quêtes</p>';
  }
}

// Fonction pour mettre à jour une quête
function updateQuestProgress(questId, newProgress) {
  const quest = currentQuests.find((q) => q.id === questId);
  if (quest) {
    quest.progress = Math.min(newProgress, quest.maxProgress);
    quest.completed = quest.progress >= quest.maxProgress;

    if (quest.typeQuete === "Monstre") {
      quest.nombreActuellementTuer = quest.progress;
    }

    displayQuests();

    // Notification si la quête est terminée
    if (quest.completed) {
      showQuestCompletedNotification(quest);
    }
  }
}

function showQuestCompletedNotification(quest) {
  const notification = document.createElement("div");
  notification.className = "quest-notification";
  notification.innerHTML = `
    <div class="quest-notification-content">
      <h3>🎉 Quête terminée !</h3>
      <p>${quest.titre}</p>
      <button onclick="this.parentElement.parentElement.remove()">OK</button>
    </div>
  `;
  document.body.appendChild(notification);

  // Style pour la notification
  if (!document.getElementById("quest-notification-styles")) {
    const style = document.createElement("style");
    style.id = "quest-notification-styles";
    style.textContent = `
      .quest-notification {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.8);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 10000;
      }
      .quest-notification-content {
        background: rgba(66, 65, 77, 0.95);
        border: 2px solid #64b5f6;
        border-radius: 12px;
        padding: 2rem;
        text-align: center;
        min-width: 300px;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
      }
      .quest-notification-content h3 {
        color: #64b5f6;
        margin-bottom: 1rem;
        font-size: 1.5rem;
      }
      .quest-notification-content p {
        margin-bottom: 1.5rem;
        color: white;
      }
      .quest-notification-content button {
        background: rgba(100, 181, 246, 0.2);
        border: 2px solid #64b5f6;
        color: white;
        padding: 0.8rem 1.5rem;
        border-radius: 8px;
        cursor: pointer;
        font-weight: bold;
        transition: all 0.3s ease;
      }
      .quest-notification-content button:hover {
        background: rgba(100, 181, 246, 0.4);
        transform: translateY(-2px);
      }
    `;
    document.head.appendChild(style);
  }
}

// Fonction pour vérifier la progression des quêtes basée sur la position
function checkLocationQuests(x, y) {
  currentQuests.forEach((quest) => {
    if (
      quest.typeQuete === "Destination" &&
      !quest.completed &&
      quest.destination &&
      quest.destination.x === x &&
      quest.destination.y === y
    ) {
      updateQuestProgress(quest.id, quest.maxProgress);
    }
  });
}

// Fonction pour vérifier la progression des quêtes basée sur les combats
function checkMonsterQuests(monsterType) {
  currentQuests.forEach((quest) => {
    if (
      quest.typeQuete === "Monstre" &&
      !quest.completed &&
      quest.typeMonstre.toLowerCase() === monsterType.toLowerCase()
    ) {
      updateQuestProgress(quest.id, quest.progress + 1);
    }
  });
}

// Fonction pour vérifier la progression des quêtes basée sur le niveau
function checkLevelQuests(currentLevel) {
  currentQuests.forEach((quest) => {
    if (quest.typeQuete === "Niveau" && !quest.completed) {
      updateQuestProgress(quest.id, currentLevel);
    }
  });
}

// Exposition des fonctions pour les autres modules
window.questsManager = {
  updateQuestProgress,
  checkLocationQuests,
  checkMonsterQuests,
  checkLevelQuests,
  loadQuests,
  getCurrentQuests: () => currentQuests,
};