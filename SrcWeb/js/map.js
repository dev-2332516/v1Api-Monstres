// GetTile: save fetched tile into mapArray
async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = "";
    const tile = await callAPI(`Tuiles/GetOrCreateTuile/${x}/${y}`, "GET");
    // update gameGrid (y-index then x-index)
    // gameGrid[y - (posY - 2)][x - (posX - 2)] = tile;
    // save into persistent map
    if (x >= 1 && x <= MAP_SIZE && y >= 1 && y <= MAP_SIZE) {
      mapArray[x - 1][y - 1] = tile;
    }
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
    if (tile.monstre) {
      let img = document.createElement("img");
      img.id = "tileMonstre";
      img.src = tile.monstre.spriteURL;
      td.appendChild(img);
    }

    getInfoTile(x, y);
    // remove any previous "get tile" handler (stable reference stored on the element)
    if (td.getTileClick) {
      td.removeEventListener("click", td.getTileClick);
      td.getTileClick = null;
    }
    // add info handler (store reference so it can be removed later)
    td.GetInfoTileClick = (function(capturedX, capturedY) {
      return function () {
        getInfoTile(capturedX, capturedY);
      };
    })(x, y);
    td.addEventListener("click", td.GetInfoTileClick);
  } catch (error) {
    console.error("Erreur API : ", error);
  }
  // await displayGameGrid();
}

function showCoordinates(c, r) {
  // Sauvegarder les coordonnées actuelles pour d'autres usages
  window.currentCoords = { x: c, y: r };
  console.log(`Position actuelle: ${c}, ${r}`);
}

// Crée la grille de jeu 5x5 et mettre les tuiles aux bonnes positions dans gameGrid sans afficher
async function createGrid() {
  const personnage = await callAPI(`Personnages/GetPersonnageFromUser/`, "GET");
  posX = personnage["positionX"];
  posY = personnage["positionY"];
  gameContainer.innerHTML = "";
  const table = document.createElement("table");
  table.id = "table";
  for (let r = 0; r < 5; r++) {
    const tr = document.createElement("tr");
    for (let c = 0; c < 5; c++) {
      const td = document.createElement("td");
      td.id = `${posX - 2 + c}; ${posY - 2 + r}`;
      td._coords = { x: posX - 2 + c, y: posY - 2 + r };
      td.getTileClick = (function(capturedTd) {
        return function () {
          GetTile(capturedTd._coords.x, capturedTd._coords.y, capturedTd);
        };
      })(td);
      td.addEventListener("click", td.getTileClick);

      const p = document.createElement("p");
      p.innerText = "?";
      td.appendChild(p);
      tr.appendChild(td);
    }
    table.appendChild(tr);
  }
  gameContainer.appendChild(table);
  await GetInitialTuiles();
}

// mettre à jour les tuiles par défaut dans gameGrid et sans les afficher
async function GetInitialTuiles() {
  try {
    const initialTiles = await callAPI("Tuiles/GetInitialTuiles", "GET");
    let count = 0;
    Array.from(table.children).forEach((tr, indexTr) => {
      Array.from(tr.children).forEach((td, indexTd) => {
        let xToGet = Number(td.id.split("; ")[0]);
        let yToGet = Number(td.id.split("; ")[1]);
        let tile = initialTiles.find((matchTile) => matchTile.positionX == xToGet && matchTile.positionY == yToGet);
        if (tile) {
          if (
            tile.positionX >= 1 &&
            tile.positionX <= MAP_SIZE &&
            tile.positionY >= 1 &&
            tile.positionY <= MAP_SIZE
          ) {
            mapArray[tile.positionX - 1][tile.positionY - 1] = tile;
          }
        }
      });
    });
  } catch (error) {
    console.error("Erreur API : ", error);
  }
  await displayGameGrid();
}

// Nettoie tous les event listeners de la grille
function clearAllEventListeners() {
  const table = document.getElementById("table");
  if (table) {
    Array.from(table.children).forEach((tr) => {
      Array.from(tr.children).forEach((td) => {
        // Supprimer tous les event listeners existants
        if (td.getTileClick) {
          td.removeEventListener("click", td.getTileClick);
          td.getTileClick = null;
        }
        if (td.GetInfoTileClick) {
          td.removeEventListener("click", td.GetInfoTileClick);
          td.GetInfoTileClick = null;
        }
      });
    });
  }
}

// affichage de la grille qui se trouve dans gameGrid
async function displayGameGrid() {
  const centerTile = mapArray[posX - 1]?.[posY - 1];
  if (centerTile) showCoordinates(centerTile.positionX, centerTile.positionY);
  
  // Nettoyer tous les event listeners existants avant de reconfigurer
  clearAllEventListeners();
  for (let x = -2; x <= 2; x++) {
    for (let y = -2; y <= 2; y++) {
      const worldX = posX + x;
      const worldY = posY + y;
      // check si la map à déjà la tile demandé
      if (
        worldX >= 1 &&
        worldX <= MAP_SIZE &&
        worldY >= 1 &&
        worldY <= MAP_SIZE
      ) {
        tile = mapArray[worldX - 1][worldY - 1];
      }
      const td = document.getElementById(`${worldX}; ${worldY}`);
      if (tile) {
        if (td) {
          // Mettre à jour les coordonnées du TD
          td._coords = { x: worldX, y: worldY };
          td.innerHTML = "";
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
          if (x == 0 && y == 0) {
            let img = document.createElement("img");
            img.id = "personage";
            img.src =
              "https://s.namemc.com/3d/skin/body.png?id=63455d7069b397c2&model=classic";
            td.appendChild(img);
          }
          td.GetInfoTileClick = (function(capturedX, capturedY) {
            return function () {
              getInfoTile(capturedX, capturedY);
            };
          })(worldX, worldY);
          td.addEventListener("click", td.GetInfoTileClick);
          if (tile.monstre) {
            let img = document.createElement("img");
            img.id = "tileMonstre";
            img.src = tile.monstre.spriteURL;
            td.appendChild(img);
          }
        }
      } else {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td != null) {
          // Mettre à jour les coordonnées du TD
          td._coords = { x: posX + x, y: posY + y };
          td.style.cssText = `background-image: `;
        }
        td.innerHTML = "?";
        td.getTileClick = (function(capturedX, capturedY, capturedTd) {
          return function () {
            GetTile(capturedX, capturedY, capturedTd);
          };
        })(posX + x, posY + y, td);
        td.addEventListener("click", td.getTileClick);
      }
    }
  }
}

// Fonction qui retourne une tuile selon les coordonnées x et y réel (pas locaux)
function getTileWithCoords(x, y) {
  return mapArray[x - 1][y - 1];
}

function setTileWithCoords(x, y, tileToSet) {
  mapArray[x - 1][y - 1] = tileToSet;
}

async function getInfoTile(x, y) {
  const tempTile = await getTileWithCoords(x, y);
  
  if (tempTile) {
    // Utiliser la nouvelle interface d'overlay pour afficher les informations
    if (window.gameInterface) {
      if (tempTile.monstre) {
        // Afficher les informations du monstre
        window.gameInterface.showMonsterInfo(tempTile.monstre);
      } else {
        // Afficher les informations de la tuile
        window.gameInterface.showTileInfo(
          tempTile.positionX, 
          tempTile.positionY, 
          tempTile.typeTuile, 
          tempTile.estTraversable
        );
      }
    }
    
    // Sauvegarder les coordonnées pour d'autres usages
    window.currentCoords = { x: tempTile.positionX, y: tempTile.positionY };
    console.log(`Position mise à jour: ${tempTile.positionX}, ${tempTile.positionY}`);
  }
}

function GetInfoTileClick(x, y) {
  getInfoTile(x, y);
}

function getTileClick(x, y) {
  GetTile(x, y);
}
