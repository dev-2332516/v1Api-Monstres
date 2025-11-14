// Add event listeners for movement buttons
document
  .getElementById("up-btn")
  .addEventListener("click", () => moveGrid("up"));
document
  .getElementById("down-btn")
  .addEventListener("click", () => moveGrid("down"));
document
  .getElementById("left-btn")
  .addEventListener("click", () => moveGrid("left"));
document
  .getElementById("right-btn")
  .addEventListener("click", () => moveGrid("right"));

let monstreBougerData;

// Bouge la grid selon la direction donnée
async function moveGrid(direction) {
  try {
    // Vérifier les limites de la carte AVANT d'essayer de bouger
    let nextX = posX, nextY = posY;
    if (direction == "up") nextY--;
    if (direction == "down") nextY++;
    if (direction == "left") nextX--;
    if (direction == "right") nextX++;
    
    // Empêcher le mouvement hors limites (carte de 0 à 50)
    if (nextX < 0 || nextX > 50 || nextY < 0 || nextY > 50) {
      showErrorPopup("Vous ne pouvez pas aller plus loin dans cette direction !");
      return;
    }
    
    // Movement réel
    if (await movePersonnage(direction)) {
      if (!isDefeated) {
        // Calculer la position cible avant le mouvement
        let targetX = posX, targetY = posY;
        if (direction == "up") targetY--;
        if (direction == "down") targetY++;
        if (direction == "left") targetX--;
        if (direction == "right") targetX++;
        
        // Si on doit supprimer un monstre, le faire AVANT de bouger
        if (deleteMonstre) {
          // Ajuster les indices pour mapArray (base 0 vs position base 1)
          let arrayX = targetX - 1;
          let arrayY = targetY - 1;
          
          let monstreTuer = mapArray[arrayX] && mapArray[arrayX][arrayY] ? mapArray[arrayX][arrayY].monstre : null;
          if (monstreTuer) {
            showErrorPopup(`Vous avez tuer ${monstreTuer.nom} !`);
            
            // Vérifier les quêtes de monstres
            if (window.questsManager) {
              window.questsManager.checkMonsterQuests(monstreTuer.type1 || monstreTuer.nom);
            }
            
            mapArray[arrayX][arrayY].monstre = null;
          }
          deleteMonstre = false;
        }
        
        // Maintenant effectuer le mouvement
        if (direction == "up") posY--;
        if (direction == "down") posY++;
        if (direction == "left") posX--;
        if (direction == "right") posX++;
        
        await shiftTable(direction);
        if ((posY < 49 && posY > 1) || (posX < 49 && posX > 1)) {
          await getNewLines(direction);
        }
        
        // Vérifier les quêtes de destination
        if (window.questsManager) {
          window.questsManager.checkLocationQuests(posX, posY);
        }
        
        await displayGameGrid();
        
        // Mettre à jour les infos si elles sont affichées (stats, carte, etc.)
        if (window.updateInfoIfVisible) {
          await window.updateInfoIfVisible();
        }
      } else if (isDefeated && !isIndecis) {
        createGrid();
        // Mettre à jour les infos si elles sont affichées (stats, carte, etc.)
        if (window.updateInfoIfVisible) {
          await window.updateInfoIfVisible();
        }
      } else if (isDefeated && isIndecis) {
        showErrorPopup("Indécis");
        let tempX = posX, tempY = posY;
        if (direction == "up") tempY--;
        if (direction == "down") tempY++;
        if (direction == "left") tempX--;
        if (direction == "right") tempX++;

        // Get le monstre dans la tile et update
        let tempTile = getTileWithCoords(tempX, tempY);
        tempTile.monstre.pointsVieActuels = monstreBougerData;
        setTileWithCoords(tempX, tempY, tempTile);
        // Mettre à jour les infos si elles sont affichées (stats, carte, etc.)
        if (window.updateInfoIfVisible) {
          await window.updateInfoIfVisible();
        }
      }
    }
  } catch (error) {
    console.error("Error moving grid:", error);
  }
}

async function movePersonnage(direction) {
  const dateVar = new Date();
  let tokenObject = JSON.parse(localStorage.getItem("jwtToken"));
  const now = Math.floor(new Date().getTime() / 1000.0);
  if (!tokenObject || tokenObject.expiry <= now) {
    document.getElementById("logout-btn").click();
  } else {
    let token = JSON.parse(tokenObject.value);
    const response = await fetch(
      `https://localhost:7223/api/Personnages/MovePersonnage/${direction}`,
      {
        method: "PUT",
        headers: {
          userToken: token.token,
        },
      }
    );
    if ((await response.status) != 200) {
      return false;
    }
    const data = await response.text();
    let dataParsed = JSON.parse(data);
    if ((await dataParsed.message) == "Moved") {
      deleteMonstre = false;
      isDefeated = false;
      isIndecis = false;
    }
    if ((await dataParsed.message) == "WonFight") {
      deleteMonstre = true;
      isDefeated = false;
      isIndecis = false;
    }
    if ((await dataParsed.message) == "LostFight") {
      deleteMonstre = false;
      isDefeated = true;
      isIndecis = false;
    }
    if ((await dataParsed.message) == "Indecis") {
      deleteMonstre = false;
      isDefeated = true;
      isIndecis = true;
      monstreBougerData = dataParsed.ptsVieMonstre;
    }
  }
  return true;
}

// Shift les IDs de la table avec la direction donnee
function shiftTable(direction) {
  const table = document.getElementById("table");
  Array.from(table.children).forEach((tr) => {
    Array.from(tr.children).forEach((td) => {
      let xToGet = td.id.split("; ")[0];
      let yToGet = td.id.split("; ")[1];
      switch (direction) {
        case "up":
          td.id = xToGet + "; " + (Number(yToGet) - 1);
          break;
        case "down":
          td.id = xToGet + "; " + (Number(yToGet) + 1);
          break;
        case "left":
          td.id = Number(xToGet) - 1 + "; " + yToGet;
          break;
        case "right":
          td.id = Number(xToGet) + 1 + "; " + yToGet;
          break;
      }
    });
  });
}

// Get les nouvelles lignes selon la direction données
async function getNewLines(direction) {
  const newTiles = await callAPI(`Tuiles/GetTuilesLine/${direction}`, "GET");
  
  // Vérifier que newTiles existe et contient des données valides
  if (!newTiles || !Array.isArray(newTiles) || newTiles.length === 0) {
    return; // Pas de nouvelles tuiles à charger (probablement aux bordures)
  }
  
  // Vérifier chaque tuile avant de l'ajouter
  for (let i = 0; i < newTiles.length; i++) {
    if (newTiles[i] && newTiles[i].positionX && newTiles[i].positionY) {
      const arrayX = newTiles[i].positionX - 1;
      const arrayY = newTiles[i].positionY - 1;
      if (!mapArray[arrayX]) mapArray[arrayX] = [];
      if (!mapArray[arrayX][arrayY]) {
        mapArray[arrayX][arrayY] = newTiles[i];
      }
    }
  }
}
