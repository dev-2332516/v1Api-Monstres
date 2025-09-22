const gameContainer = document.getElementById("grid-container");
let posX = 10;
let posY = 10;

let gameGrid = Array.from({ length: 5 }, () => Array(5).fill(null));

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

createGrid();

//displayDefaultTiles();

async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = "";
    console.log(`Fetching tile at (${x}, ${y})`);
    const response = await fetch(
      `https://localhost:7223/api/Tuiles/GetOrCreateTuile/${x}/${y}`
    );
    const tile = await response.json();
    gameGrid[x - (posX - 2)][y - (posY - 2)] = tile; // Met à jour la tuile dans gameGrid
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
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
  gameContainer.innerHTML = "";
  const table = document.createElement("table");
  table.id = "table";
  for (let r = 0; r < 5; r++) {
    const tr = document.createElement("tr");
    for (let c = 0; c < 5; c++) {
      const td = document.createElement("td");
      td.id = `${posX - 2 + c}; ${posY - 2 + r}`;
      td.addEventListener("click", () =>
        GetTile(posX - 2 + r, posY - 2 + c, td)
      );
      //td.addEventListener('click', () => showCoordinates(posX + c, posY + r));

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
    const response = await fetch(
      `https://localhost:7223/api/Tuiles/GetInitialTuiles/${posX}/${posY}`
    );
    const initialTiles = await response.json();
    let count = 0;
    for (let y = 0; y <= 4; y++) {
      for (let x = 0; x <= 4; x++) {
        gameGrid[x][y] = initialTiles[count];
        count++;
      }
    }
  } catch (error) {
    console.error("Erreur API : ", error);
  }
  await displayGameGrid();
}

// affichage de la grille qui se trouve dans gameGrid
async function displayGameGrid() {
  for (let x = -2; x <= 2; x++) {
    for (let y = -2; y <= 2; y++) {
      const tile = gameGrid[y + 2][x + 2];
      if (tile) {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td) {
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
        }
      } else {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td != null) td.style.cssText = `background-image: `;
      }
    }
  }
}

document.addEventListener("DOMContentLoaded", () => {
  // Gestion du formulaire de connexion
  const loginForm = document.getElementById("login-form");
  if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const username = document.getElementById("login-username").value;
      const password = document.getElementById("login-password").value;
      try {
        const response = await fetch("https://localhost:7223/api/auth/login", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ username, password }),
        });
        if (response.ok) {
          //const data = await response.json();
          //localStorage.setItem('token', data.token);
          window.location.href = "index.html";
        } else {
          alert("Identifiants invalides !");
        }
      } catch (err) {
        alert("Erreur de connexion au serveur.");
      }
    });
  }

  // Gestion du formulaire d'inscription
  const registerForm = document.getElementById("register-form");
  if (registerForm) {
    registerForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = document.getElementById("register-email").value;
      const username = document.getElementById("register-username").value;
      const password = document.getElementById("register-password").value;
      try {
        const response = await fetch(
          "https://localhost:7223/api/auth/register",
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email,
              username,
              password,
            }),
          }
        );
        if (response.ok) {
          alert("Inscription réussie, connecte-toi !");
          window.location.href = "login.html";
        } else {
          alert("Erreur lors de l'inscription.");
        }
      } catch (err) {
        alert("Erreur de connexion au serveur.");
      }
    });
  }
});

async function moveGrid(direction) {
  try {

    if (direction == "up") posY--;
    if (direction == "down") posY++;
    if (direction == "left") posX--;
    if (direction == "right") posX++;
    // Update position based on direction
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
        for (let c = 0; c <= 4; c++) {
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

    // Get new lines
    const response = await fetch(
      `https://localhost:7223/api/Tuiles/GetTuilesLine/${posX}/${posY}/${direction}`
    );
    const newTiles = await response.json();
    switch (direction) {
      case "up":
        gameGrid[1][1] = newTiles[0];
        gameGrid[1][2] = newTiles[1];
        gameGrid[1][3] = newTiles[2];
        break;
      case "down":
        gameGrid[3][1] = newTiles[0];
        gameGrid[3][2] = newTiles[1];
        gameGrid[3][3] = newTiles[2];
        break;
      case "left":
        gameGrid[1][1] = newTiles[0];
        gameGrid[2][1] = newTiles[1];
        gameGrid[3][1] = newTiles[2];
        break;
      case "right":
        gameGrid[1][3] = newTiles[0];
        gameGrid[2][3] = newTiles[1];
        gameGrid[3][3] = newTiles[2];
        break;
    }

    await displayGameGrid();
    //Update tiles with new data
  } catch (error) {
    console.error("Error moving grid:", error);
    showErrorPopup("Failed to move grid");
  }
}

function showErrorPopup(message) {
  const popup = document.createElement("div");
  popup.className = "error-popup";
  popup.innerHTML = `
  <div class="error-content">
    <p>${message}</p>
    <button onclick="this.parentElement.parentElement.remove()">OK</button>
  </div>
  `;
  document.body.appendChild(popup);

  const style = document.createElement("style");
  document.head.appendChild(style);
}
