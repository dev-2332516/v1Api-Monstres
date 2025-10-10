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
        await shiftGameGrid(direction);
        if ((posY < 49 && posY > 1) || (posX < 49 && posX > 1)) {
          await getNewLines(direction);
        }
        if (deleteMonstre) {
          let monstreTuer = gameGrid[2][2].monstre;
          showErrorPopup(`Vous avez tuer ${monstreTuer.nom} !`);
          gameGrid[2][2].monstre = null;
          deleteMonstre = false;
        }
        await displayGameGrid();
      } else if (isDefeated) {
        setPersonnage();
        createGrid();
      }
    }
  } catch (error) {
    console.error("Error moving grid:", error);
    showErrorPopup("Failed to move grid");
  }
}

async function movePersonnage(direction) {
  const response = await fetch(
    `https://localhost:7223/api/Personnages/MovePersonnage/${direction}`,
    {
      method: "PUT",
      headers: {
        userToken: JSON.parse(localStorage.getItem("jwtToken")).token,
      },
    }
  );
  if ((await response.status) != 200) {
    return false;
  }
  const data = await response.text();
  if ((await data) == "WonFight") {
    deleteMonstre = true;
  }
  if ((await data) == "LostFight") {
    isDefeated = true;
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

// Shift la list game grid avec la direction donnee
function shiftGameGrid(direction) {
  const tempGameGrid = JSON.parse(JSON.stringify(gameGrid));
  switch (direction) {
    case "left":
      gameGrid[0][1] = null;
      gameGrid[1][1] = null;
      gameGrid[2][1] = null;
      gameGrid[3][1] = null;
      gameGrid[4][1] = null;
      for (let r = 4; r >= 0; r--) {
        for (let c = 0; c <= 4; c++) {
          gameGrid[r][c] = tempGameGrid[r][c - 1];
        }
      }
      break;
    case "right":
      for (let c = 0; c < 4; c++) {
        for (let r = 0; r <= 4; r++) {
          gameGrid[r][c + 1] = null;
          gameGrid[r][c] = tempGameGrid[r][c + 1];
        }
      }
      break;
    case "up":
      gameGrid[0][0] = null;
      gameGrid[0][1] = null;
      gameGrid[0][2] = null;
      gameGrid[0][3] = null;
      gameGrid[0][4] = null;
      for (let r = 4; r >= 0; r--) {
        for (let c = 0; c <= 4; c++) {
          if (r > 0) gameGrid[r][c] = tempGameGrid[r - 1][c];
        }
      }
      break;
    case "down":
      for (let c = 0; c <= 4; c++) {
        for (let r = 0; r <= 4; r++) {
          if (r < 4) {
            gameGrid[r + 1][c] = null;
            gameGrid[r][c] = tempGameGrid[r + 1][c];
          }
        }
      }
      break;
  }
}

// Get les nouvelles lignes selon la direction données
async function getNewLines(direction) {
  const newTiles = await callAPI(`Tuiles/GetTuilesLine/${direction}`, "GET");
  switch (direction) {
    case "up":
      if (gameGrid[1][1] == null) gameGrid[1][1] = newTiles[0];
      if (gameGrid[1][2] == null) gameGrid[1][2] = newTiles[1];
      if (gameGrid[1][3] == null) gameGrid[1][3] = newTiles[2];
      break;
    case "down":
      if (gameGrid[3][1] == null) gameGrid[3][1] = newTiles[0];
      if (gameGrid[3][2] == null) gameGrid[3][2] = newTiles[1];
      if (gameGrid[3][3] == null) gameGrid[3][3] = newTiles[2];
      break;
    case "left":
      if (gameGrid[1][1] == null) gameGrid[1][1] = newTiles[0];
      if (gameGrid[2][1] == null) gameGrid[2][1] = newTiles[1];
      if (gameGrid[3][1] == null) gameGrid[3][1] = newTiles[2];
      break;
    case "right":
      if (gameGrid[1][3] == null) gameGrid[1][3] = newTiles[0];
      if (gameGrid[2][3] == null) gameGrid[2][3] = newTiles[1];
      if (gameGrid[3][3] == null) gameGrid[3][3] = newTiles[2];
      break;
  }
}