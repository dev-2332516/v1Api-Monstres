createGrid();

// GetTile: save fetched tile into mapArray
async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = "";
    console.log(`Fetching tile at (${x}, ${y})`);
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

    // remove any previous "get tile" handler (stable reference stored on the element)
    if (td.getTileClick) {
      td.removeEventListener("click", td.getTileClick);
      td.getTileClick = null;
    }
    // add info handler (store reference so it can be removed later)
    td.GetInfoTileClick = function () {
      getInfoTile(x, y);
    };
    td.addEventListener("click", td.GetInfoTileClick);
  } catch (error) {
    console.error("Erreur API : ", error);
  }
  // await displayGameGrid();
}

function showCoordinates(c, r) {
  const coord = document.getElementById("coord");
  coord.innerText = "";
  coord.innerHTML = c + ", " + r;
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
      td.getTileClick = function () {
        GetTile(td._coords.x, td._coords.y, td);
      };
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

// affichage de la grille qui se trouve dans gameGrid
async function displayGameGrid() {
  const centerTile = mapArray[posX - 1]?.[posY - 1];
  if (centerTile) showCoordinates(centerTile.positionX, centerTile.positionY);
  for (let x = -2; x <= 2; x++) {
    for (let y = -2; y <= 2; y++) {
      // let tile = gameGrid[y + 2][x + 2];
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
          td.innerHTML = "";
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
          if (x == 0 && y == 0) {
            let img = document.createElement("img");
            img.id = "personage";
            img.src =
              "https://s.namemc.com/3d/skin/body.png?id=63455d7069b397c2&model=classic";
            td.appendChild(img);
          }
          if (tile.monstre) {
            let img = document.createElement("img");
            img.id = "tileMonstre";
            img.src = tile.monstre.spriteURL;
            td.appendChild(img);
            continue;
          }
          if (td.getTileClick) {
            td.removeEventListener("click", td.getTileClick);
            td.getTileClick = null;
          }
          if (!td.GetInfoTileClick) {
            td.GetInfoTileClick = function () {
              getInfoTile(worldX, worldY);
            };
            td.addEventListener("click", td.GetInfoTileClick);
          }
        }
      } else {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td != null) td.style.cssText = `background-image: `;
        td.innerHTML = "?";
        if (td.GetInfoTileClick) {
          td.removeEventListener("click", td.GetInfoTileClick);
          td.GetInfoTileClick = null;
        }
        if (!td.getTileClick) {
          td.getTileClick = function () {
            GetTile(posX + x, posY + y, td);
          };
          td.addEventListener("click", td.getTileClick);
        }
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
  document.getElementById("coord-sel").innerHTML =
    tempTile["positionX"] + "," + tempTile["positionY"];
  document.getElementById("tuile-type").innerHTML = tempTile.typeTuile;
  document.getElementById("is-traversable").innerHTML =
    tempTile["estTraversable"];
  if (tempTile.monstre) {
    setInfoMonster(tempTile.monstre);
  }
}

function GetInfoTileClick(x, y) {
  getInfoTile(x, y);
}

function getTileClick(x, y) {
  GetTile(x, y);
}
