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
    // Movement réel
    if (await movePersonnage(direction)) {
      if (!isDefeated) {
        if (direction == "up") posY--;
        if (direction == "down") posY++;
        if (direction == "left") posX--;
        if (direction == "right") posX++;
        await shiftTable(direction);
        if ((posY < 49 && posY > 1) || (posX < 49 && posX > 1)) {
          await getNewLines(direction);
        }
        if (deleteMonstre) {
          let monstreTuer = mapArray[posX][posY].monstre;
          showErrorPopup(`Vous avez tuer ${monstreTuer.nom} !`);
          mapArray[posX][posY].monstre = null;
          deleteMonstre = false;
        }
        await displayGameGrid();
      } else if (isDefeated && !isIndecis) {
        setPersonnage();
        createGrid();
      } else if (isDefeated && isIndecis) {
        showErrorPopup("Indécis");
        setPersonnage();
        let tempX = posX, tempY = posY;
        if (direction == "up") tempY--;
        if (direction == "down") tempY++;
        if (direction == "left") tempX--;
        if (direction == "right") tempX++;

        // Get le monstre dans la tile et update
        let tempTile = getTileWithCoords(tempX, tempY);
        tempTile.monstre.pointsVieActuels = monstreBougerData;
        setTileWithCoords(tempX, tempY, tempTile);
      }
    }
  } catch (error) {
    console.error("Error moving grid:", error);
    showErrorPopup("Failed to move grid");
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
    }
    if ((await dataParsed.message) == "LostFight") {
      isDefeated = true;
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
  if (!mapArray[newTiles[0].positionX - 1][newTiles[0].positionY - 1]) mapArray[newTiles[0].positionX - 1][newTiles[0].positionY - 1] = newTiles[0];
  if (!mapArray[newTiles[1].positionX - 1][newTiles[1].positionY - 1]) mapArray[newTiles[1].positionX - 1][newTiles[1].positionY - 1] = newTiles[1];
  if (!mapArray[newTiles[2].positionX - 1][newTiles[2].positionY - 1]) mapArray[newTiles[2].positionX - 1][newTiles[2].positionY - 1] = newTiles[2];
}
