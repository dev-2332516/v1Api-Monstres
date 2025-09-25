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
    let token = localStorage.getItem("jwtToken");
    const response = await fetch(
      `https://localhost:7223/api/Tuiles/GetOrCreateTuile/${x}/${y}`,
      {
        headers: { "userToken": token },
      }
    );
    const tile = await response.json();
    gameGrid[y - (posY - 2)][x - (posX - 2)] = tile; // Met à jour la tuile dans gameGrid
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
        GetTile(posX - 2 + c, posY - 2 + r, td)
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
    let token = localStorage.getItem("jwtToken");
    const response = await fetch(
      `https://localhost:7223/api/Tuiles/GetInitialTuiles/`, {
        method: "GET",
        headers: {
           "userToken": token 
          },
      },
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
  showCoordinates(gameGrid[2][2].positionX, gameGrid[2][2].positionY);
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

// Bouge la grid selon la direction donnée
async function moveGrid(direction) {
  try {
    if (direction == "up") posY--;
    if (direction == "down") posY++;
    if (direction == "left") posX--;
    if (direction == "right") posX++;
    // Movement réel
    await shiftTable(direction);
    await shiftGameGrid(direction);
    await getNewLines(direction);
    await displayGameGrid();
  } catch (error) {
    console.error("Error moving grid:", error);
    showErrorPopup("Failed to move grid");
  }
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
  const response = await fetch(
    `https://localhost:7223/api/Tuiles/GetTuilesLine/${gameGrid[2][2].positionX}/${gameGrid[2][2].positionY}/${direction}`,
    {
      headers: { "userToken": localStorage.getItem("jwtToken") },
    }
  );
  const newTiles = await response.json();
  switch (direction) {
    case "up":
      if (gameGrid[1][1] == null) gameGrid[1][1] = newTiles[2];
      if (gameGrid[1][2] == null) gameGrid[1][2] = newTiles[1];
      if (gameGrid[1][3] == null) gameGrid[1][3] = newTiles[0];
      break;
    case "down":
      if (gameGrid[3][1] == null) gameGrid[3][1] = newTiles[2];
      if (gameGrid[3][2] == null) gameGrid[3][2] = newTiles[1];
      if (gameGrid[3][3] == null) gameGrid[3][3] = newTiles[0];
      break;
    case "left":
      if (gameGrid[1][1] == null) gameGrid[1][1] = newTiles[2];
      if (gameGrid[2][1] == null) gameGrid[2][1] = newTiles[1];
      if (gameGrid[3][1] == null) gameGrid[3][1] = newTiles[0];
      break;
    case "right":
      if (gameGrid[1][3] == null) gameGrid[1][3] = newTiles[2];
      if (gameGrid[2][3] == null) gameGrid[2][3] = newTiles[1];
      if (gameGrid[3][3] == null) gameGrid[3][3] = newTiles[0];
      break;
  }
}

document.addEventListener("DOMContentLoaded", () => {
  // Gestion du formulaire de connexion
  const loginForm = document.getElementById("login-form");
  if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = document.getElementById("email-login").value;
      const password = document.getElementById("password-login").value;
      try {
        const response = await fetch(
          `https://localhost:7223/api/utilisateurs/login/${email}/${password}`,
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
          }
        );
        if (response.ok) {
          const token = await response.text(); // La réponse renvoie seulement le token JWT
          localStorage.setItem("jwtToken", token);
          updateUIBasedOnAuth();
          document.getElementById("login-message").textContent =
            "Connexion réussie !";
        } else {
          const errorText = await response.text();
          document.getElementById("login-message").textContent =
            "Erreur: " + errorText;
        }
      } catch (err) {
        document.getElementById("login-message").textContent =
          "Erreur de connexion au serveur.";
        console.error("Erreur login:", err);
      }
    });
  }

  // Gestion du formulaire d'inscription
  const registerForm = document.getElementById("register-form");
  if (registerForm) {
    registerForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = document.getElementById("email-register").value;
      const pseudo = document.getElementById("pseudo-register").value;
      const password = document.getElementById("password-register").value;
      try {
        const response = await fetch(
          "https://localhost:7223/api/utilisateurs/register",
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email,
              pseudo,
              password,
            }),
          }
        );
        if (response.ok) {
          document.getElementById("register-message").textContent =
            "Inscription réussie ! Vous pouvez maintenant vous connecter.";
          // Basculer vers le formulaire de login après inscription réussie
          document.getElementById("register-box").style.display = "none";
          document.getElementById("login-box").style.display = "block";
        } else {
          const errorText = await response.text();
          document.getElementById("register-message").textContent =
            "Erreur: " + errorText;
        }
      } catch (err) {
        document.getElementById("register-message").textContent =
          "Erreur de connexion au serveur.";
        console.error("Erreur register:", err);
      }
    });
  }
});

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

function isUserLoggedIn() {
  return !!localStorage.getItem("jwtToken");
}

// Fonction pour mettre à jour l'UI selon l'état d'authentification
function updateUIBasedOnAuth() {
  const logoutBtn = document.getElementById("logout-btn");

  if (isUserLoggedIn()) {
    // Utilisateur connecté : cacher le formulaire, afficher le jeu et le bouton logout
    document.getElementById("auth-container").style.display = "none";
    document.getElementById("game-container").style.display = "flex";
    if (logoutBtn) {
      logoutBtn.style.display = "block";
    }
  } else {
    // Pas connecté : affichage formulaire, cacher jeu et bouton logout
    document.getElementById("auth-container").style.display = "block";
    document.getElementById("game-container").style.display = "none";
    if (logoutBtn) {
      logoutBtn.style.display = "none";
    }
  }
}

// Gestionnaire de logout
document.getElementById("logout-btn").addEventListener("click", async () => {
  try {
    // Supprimer le token du localStorage
    localStorage.removeItem("jwtToken");

    // Mettre à jour l'UI : masquer le jeu, afficher le formulaire de connexion
    updateUIBasedOnAuth();

    // Réinitialiser les messages
    document.getElementById("login-message").textContent = "";
    if (document.getElementById("register-message")) {
      document.getElementById("register-message").textContent = "";
    }

    console.log("Déconnexion réussie");
  } catch (err) {
    console.error("Erreur lors de la déconnexion:", err);
    alert("Impossible de se déconnecter.");
  }
});

// Gestionnaire pour basculer entre login et register
document.addEventListener("DOMContentLoaded", () => {
  // Vérifier l'état d'auth au chargement
  updateUIBasedOnAuth();

  // Toggle vers register
  const showRegisterBtn = document.getElementById("show-register");
  if (showRegisterBtn) {
    showRegisterBtn.addEventListener("click", () => {
      document.getElementById("login-box").style.display = "none";
      document.getElementById("register-box").style.display = "block";
    });
  }

  // Toggle vers login
  const showLoginBtn = document.getElementById("show-login");
  if (showLoginBtn) {
    showLoginBtn.addEventListener("click", () => {
      document.getElementById("register-box").style.display = "none";
      document.getElementById("login-box").style.display = "block";
    });
  }
});
