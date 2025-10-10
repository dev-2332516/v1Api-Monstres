createGrid();

// GetTile: save fetched tile into mapArray
async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = "";
    console.log(`Fetching tile at (${x}, ${y})`);
    const tile = await callAPI(`Tuiles/GetOrCreateTuile/${x}/${y}`, "GET");
    // update gameGrid (y-index then x-index)
    gameGrid[y - (posY - 2)][x - (posX - 2)] = tile;
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
    td.removeEventListener("click", getTileClick(x, y));
    td.addEventListener("click", GetInfoTileClick(x, y));
  } catch (error) {
    console.error("Erreur API : ", error);
  }
  await displayGameGrid();
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
      td.addEventListener("click", function getTileClick() {
        GetTile(posX - 2 + c, posY - 2 + r, td);
      });
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
        let tile = initialTiles.find(
          (matchTile) =>
            matchTile.positionX == xToGet && matchTile.positionY == yToGet
        );
        if (tile) {
          gameGrid[indexTr][indexTd] = tile;
          // save to map
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
  // center tile coords should exist; guard if not
  const centerTile = gameGrid[2][2] || mapArray[posX - 1]?.[posY - 1];
  if (centerTile) showCoordinates(centerTile.positionX, centerTile.positionY);
  for (let x = -2; x <= 2; x++) {
    for (let y = -2; y <= 2; y++) {
      // gameGrid row = y+2, col = x+2
      let tile = gameGrid[y + 2][x + 2];
      const worldX = posX + x;
      const worldY = posY + y;
      // if no tile in current viewport cache, try mapArray
      if (
        !tile &&
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
          td.removeEventListener("click", function getTileClick() {
            GetTile(posX - 2 + c, posY - 2 + r, td);
          });
          td.addEventListener("click", function GetInfoTileClick() {
            getInfoTile(td.id.split("; ")[0], td.id.split("; ")[1]);
          });
        }
      } else {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td != null) td.style.cssText = `background-image: `;
        td.innerHTML = "";
        td.removeEventListener("click", function getTileClick() {
          GetTile(posX - 2 + c, posY - 2 + r, td);
        });
        td.addEventListener("click", function GetInfoTileClick() {
          getInfoTile(td.id.split("; ")[0], td.id.split("; ")[1]);
        });
      }
    }
  }
}

function getTileWithCoords(x, y) {
  let tempTile = null;
  for (let i = 0; i < gameGrid.length; i++) {
    const line = gameGrid[i];
    for (let j = 0; j < line.length; j++) {
      const tile = line[j];
      if (tile && tile.positionX == x && tile.positionY == y) {
        tempTile = tile;
        return tempTile;
      }
    }
  }
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
