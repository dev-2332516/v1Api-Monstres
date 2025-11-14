// Interface simplifiée - Gestion des overlays et actions
document.addEventListener("DOMContentLoaded", () => {
  initializeInterface();
  scheduleQuestUpdate();
});

function initializeInterface() {
  const overlay = document.getElementById("info-overlay");
  if (overlay) overlay.classList.add("hidden");
}

// Gestion de l'overlay d'informations
function hideInfoOverlay() {
  const overlay = document.getElementById("info-overlay");
  if (overlay) overlay.classList.add("hidden");
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

async function showStats() {
  try {
    const personnage = await callAPI(`Personnages/GetPersonnageFromUser/`, "GET");
    const coords = window.currentCoords || { x: personnage.positionX, y: personnage.positionY };
    
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
        <span>${coords.x}, ${coords.y}</span>
      </div>
    `;
    showInfoOverlay("📊 Statistiques du Joueur", statsContent);
  } catch (error) {
    showInfoOverlay("📊 Statistiques du Joueur", "<p>Erreur de chargement</p>");
  }
}

async function updateInfoIfVisible() {
  const overlay = document.getElementById("info-overlay");
  const title = document.getElementById("info-title");
  
  if (overlay && !overlay.classList.contains("hidden") && title) {
    if (title.textContent === "📊 Statistiques du Joueur") await showStats();
    else if (title.textContent === "🗺️ Informations Carte") showMap();
    else if (title.textContent === "📋 Quêtes Actives") await showQuests();
    else if (title.textContent === "👹 Informations Monstre") await updateMonsterInfo();
  }
}

async function updateMonsterInfo() {
  try {
    // Récupérer la position actuelle du joueur
    const coords = window.currentCoords;
    if (!coords) return;
    
    // Récupérer les informations de la tuile actuelle
    if (window.getTileWithCoords) {
      const currentTile = await window.getTileWithCoords(coords.x, coords.y);
      if (currentTile && currentTile.monstre) {
        showMonsterInfo(currentTile.monstre);
      } else {
        // Plus de monstre sur cette tuile, fermer l'overlay
        hideInfoOverlay();
      }
    }
  } catch (error) {
    console.error("Erreur lors de la mise à jour des informations du monstre:", error);
  }
}

async function showQuests() {
  try {
    const personnage = await callAPI(`Personnages/GetPersonnageFromUser/`, "GET");
    const quests = await callAPI(`Quetes/GetQuetes/${personnage.id}`, "GET");
    
    let questsContent = quests?.length > 0 
      ? '<div>' + quests.map(quest => {
          const icon = getQuestIconByType(quest.typeQuete || quest.TypeQuete);
          const title = quest.titre || quest.Titre || quest.nom || 'Quête';
          const progress = formatQuestProgress(quest);
          return `<div style="background: rgba(45, 45, 68, 0.6); padding: 0.8rem; border-radius: 6px; margin-bottom: 0.5rem;">
            <div style="color: #64b5f6; font-weight: bold;">${icon} ${title}</div>
            <div style="font-size: 0.85rem; color: rgba(255, 255, 255, 0.6);">${progress}</div>
          </div>`;
        }).join('') + '</div>'
      : '<div style="text-align: center; padding: 2rem;">📋<br>Aucune quête active</div>';
    
    showInfoOverlay("📋 Quêtes Actives", questsContent);
  } catch (error) {
    showInfoOverlay("📋 Quêtes Actives", "<div style='text-align: center; padding: 2rem;'>⚠️<br>Erreur</div>");
  }
}

// Fonctions utilitaires pour les quêtes
function getQuestIconByType(typeQuete) {
  if (typeof typeQuete === 'number') {
    switch (typeQuete) {
      case 0: return "🗺️"; // Destination
      case 1: return "⚔️"; // Monstre
      case 2: return "📈"; // Niveau
      default: return "❓";
    }
  }
  
  if (typeQuete && typeof typeQuete === 'string') {
    switch (typeQuete.toLowerCase()) {
      case "destination": return "🗺️";
      case "monstre": return "⚔️";
      case "niveau": return "📈";
      default: return "❓";
    }
  }
  return "❓";
}

function formatQuestProgress(quest) {
  if (quest.completed) return "Terminé ✓";
  
  const questType = quest.typeQuete || quest.TypeQuete;
  
  if (typeof questType === 'number') {
    switch (questType) {
      case 0: // Destination
        return `Se rendre en (${quest.destinationX || '?'}, ${quest.destinationY || '?'})`;
      case 1: // Monstre
        return `${quest.nombreActuellementTuer || 0}/${quest.nombreATuer || '?'}`;
      case 2: // Niveau
        return `Vous êtes actuellement au niveau ${quest.niveauSauvegarder}`;
    }
  }
  
  return `${quest.progress || 0}/${quest.maxProgress || '?'}`;
}

function getQuestDescription(quest) {
  const questType = quest.typeQuete || quest.TypeQuete;
  
  if (typeof questType === 'number') {
    switch (questType) {
      case 0: return `Se rendre aux coordonnées (${quest.destinationX || '?'}, ${quest.destinationY || '?'})`;
      case 1: return `Éliminez ${quest.nombreATuer || '?'} ${quest.typeMonstre || 'monstres'}`;
      case 2: return `Atteignez le niveau ${quest.niveauCible || '?'}`;
      default: return quest.description || "Objectif inconnu";
    }
  }
  
  return quest.description || "Objectif inconnu";
}

// Variables globales pour le système de timestamp
let questTimestamp = null;
let questUpdateTimer = null;

// Fonction pour programmer les mises à jour des quêtes basées sur le timestamp
async function scheduleQuestUpdate() {
  if (questUpdateTimer) clearTimeout(questUpdateTimer);
  
  const tokenObject = JSON.parse(localStorage.getItem("jwtToken") || "null");
  const now = Math.floor(new Date().getTime()/1000.0);
  
  if (!tokenObject || tokenObject.expiry <= now) return;
  
  try {
    const timestampData = await callAPI(`ServiceTimestamps/GetTimestamp/`, "GET");
    
    if (timestampData !== null && timestampData !== undefined) {
      let timestampMs = timestampData > 1000000000000 ? timestampData : timestampData * 1000;
      const targetTime = new Date(timestampMs);
      const delay = targetTime.getTime() - new Date().getTime();
      
      if (delay > 0 && delay < 24 * 60 * 60 * 1000) {
        questUpdateTimer = setTimeout(() => {
          updateQuestsIfVisible();
          scheduleQuestUpdate();
        }, delay);
      }
      
      questTimestamp = timestampData;
    }
  } catch (error) {
    if (!error.message?.includes('404')) {
      questUpdateTimer = setTimeout(() => {
        scheduleQuestUpdate();
      }, 300000);
    }
  }
}

async function updateQuestsIfVisible() {
  const overlay = document.getElementById("info-overlay");
  const title = document.getElementById("info-title");
  
  if (overlay && !overlay.classList.contains("hidden") && 
      title && title.textContent === "📋 Quêtes Actives") {
    await showQuests();
  }
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
  `;
  showInfoOverlay("🗺️ Informations Carte", mapContent);
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
      <span>Type1:</span>
      <span>${monster.type1 || "Aucun"}</span>
    </div>
    <div class="info-row">
      <span>Type2:</span>
      <span>${monster.type2 || "Aucun"}</span>
    </div>
  `;
  showInfoOverlay("👹 Informations Monstre", monsterContent);
}

// Exposer les fonctions pour les autres modules
window.gameInterface = {
  showTileInfo,
  showMonsterInfo,
  hideInfoOverlay
};

// Exposition des fonctions de mise à jour
window.updateInfoIfVisible = updateInfoIfVisible;
window.updateQuestsIfVisible = updateQuestsIfVisible;
window.updateMonsterInfo = updateMonsterInfo;
window.scheduleQuestUpdate = scheduleQuestUpdate;