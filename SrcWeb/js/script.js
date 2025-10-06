const gameContainer = document.getElementById("grid-container");
let posX = 10;
let posY = 10;

let gameGrid = Array.from({ length: 5 }, () => Array(5).fill(null));

let deleteMonstre = false;
let isDefeated = false;

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
setPersonnage();
createGrid();

//displayDefaultTiles();

async function callAPI(route, method, responseType) {
  let token = JSON.parse(localStorage.getItem("jwtToken"));
  let response = await fetch(`https://localhost:7223/api/${route}`, {
    method: method,
    headers: { userToken: token.token },
  });
  if (responseType == "text") return (result = await response.text());
  if (responseType == "json" || !responseType)
    return (result = await response.json());
}

async function setPersonnage() {
  const personnage = await callAPI(`Personnages/GetPersonnageFromUser/`, "GET");
  posX = personnage["positionX"];
  posY = personnage["positionY"];
  document.getElementById("stat-hp").innerHTML =
    personnage["pointsVie"] + "/" + personnage["pointsVieMax"];
  document.getElementById("stat-level").innerHTML = personnage["niveau"];
  document.getElementById("stat-xp").innerHTML = personnage["experience"];
  document.getElementById("stat-str").innerHTML = personnage["force"];
  document.getElementById("stat-def").innerHTML = personnage["defense"];
}

async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = "";
    console.log(`Fetching tile at (${x}, ${y})`);
    const tile = await callAPI(`Tuiles/GetOrCreateTuile/${x}/${y}`, "GET");
    gameGrid[y - (posY - 2)][x - (posX - 2)] = tile; // Met à jour la tuile dans gameGrid
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
    if (tile.monstre) {
      let img = document.createElement("img");
      img.id = "tileMonstre";
      img.src = tile.monstre.spriteURL;
      td.appendChild(img);
    }
    td.removeEventListener("click", () =>
      GetTile(posX - 2 + c, posY - 2 + r, td)
    );
    td.addEventListener("click", () => getInfoTile(x, y));
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
      td.addEventListener("click", () =>
        GetTile(posX - 2 + c, posY - 2 + r, td)
      );
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
        let xToGet = td.id.split("; ")[0];
        let yToGet = td.id.split("; ")[1];
        let tile = initialTiles.find(
          (matchTile) =>
            matchTile.positionX == xToGet && matchTile.positionY == yToGet
        );
        if (tile) {
          gameGrid[indexTr][indexTd] = tile;
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
  showCoordinates(gameGrid[2][2].positionX, gameGrid[2][2].positionY);
  for (let x = -2; x <= 2; x++) {
    for (let y = -2; y <= 2; y++) {
      const tile = gameGrid[y + 2][x + 2];
      if (tile) {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td) {
          td.innerHTML = "";
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
          if (x == 0 && y == 0) {
            let img = document.createElement("img");
            img.id = "personage";
            img.src = "https://s.namemc.com/3d/skin/body.png?id=63455d7069b397c2&model=classic";
            td.appendChild(img);
          }
          if (tile.monstre) {
            let img = document.createElement("img");
            img.id = "tileMonstre";
            img.src = tile.monstre.spriteURL;
            td.appendChild(img);
            continue;
          }
          td.removeEventListener("click", () =>
            GetTile(posX - 2 + c, posY - 2 + r, td)
          );
          td.addEventListener("click", () =>
            getInfoTile(td.id.split("; ")[0], td.id.split("; ")[1])
          );
        }
      } else {
        const td = document.getElementById(`${posX + x}; ${posY + y}`);
        if (td != null) td.style.cssText = `background-image: `;
        td.innerHTML = "";
      }
    }
  }
}

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
  let tempTile = await getTileWithCoords(x, y);
  document.getElementById("coord-sel").innerHTML =
    tempTile["positionX"] + "," + tempTile["positionY"];
  document.getElementById("tuile-type").innerHTML = tempTile.typeTuile;
  document.getElementById("is-traversable").innerHTML =
    tempTile["estTraversable"];
  if (tempTile.monstre) {
    setInfoMonster(tempTile.monstre);
  }
}

async function setInfoMonster(monstre) {
  document.getElementById("monstre-nom").innerHTML = monstre.nom;
  document.getElementById("monstre-pv").innerHTML =
    monstre.pointsVieActuels + "/" + monstre.pointsVieMax;
  document.getElementById("monstre-force").innerHTML = monstre.force;
  document.getElementById("monstre-defense").innerHTML = monstre.defense;
  document.getElementById("monstre-niveau").innerHTML = monstre.niveau;
  document.getElementById("monstre-xp").innerHTML = monstre.experience;
  document.getElementById("monstre-type1").innerHTML = monstre.type1;
  if (monstre.type2)
    document.getElementById("monstre-type2").innerHTML = monstre.type2;
}
