const gameContainer = document.getElementById('grid-container')
let posX = 10;
let posY = 10;

let gameGrid = Array.from({ length: 5 }, () => Array(5).fill(null));



// Add event listeners for movement buttons
document.getElementById('up-btn').addEventListener('click', () => moveGrid('up'));
document.getElementById('down-btn').addEventListener('click', () => moveGrid('down')); 
document.getElementById('left-btn').addEventListener('click', () => moveGrid('left'));
document.getElementById('right-btn').addEventListener('click', () => moveGrid('right'));


createGrid();

//displayDefaultTiles();

async function GetTile(x, y, td) {
  try {
    // Remove the inner text from TD
    td.innerHTML = '';
    console.log(`Fetching tile at (${x}, ${y})`);
    const response = await fetch(`https://localhost:7223/api/Tuiles/GetOrCreateTuile/${x}/${y}`);
    const tile = await response.json();
    gameGrid[y - (posY - 2)][x - (posX - 2)] = tile; // Met à jour la tuile dans gameGrid
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
  } catch (error) {
    console.error('Erreur API : ', error)
  }
  await displayGameGrid();
}

function showCoordinates(c, r) { 
  const coord = document.getElementById("coord");
  coord.innerText = '';
  coord.innerHTML = c + ", " + r;
}

// Crée la grille de jeu 5x5 et mettre les tuiles aux bonnes positions dans gameGrid sans afficher
async function createGrid() {
  gameContainer.innerHTML = '';
  const table = document.createElement('table');
  for (let r = 0; r < 5; r++) {
    const tr = document.createElement('tr');
    for (let c = 0; c < 5; c++) {
      const td = document.createElement('td');
      td.id = `tile-${posY - 2 + c}-${posX - 2 + r}`;
      td.addEventListener('click', () => GetTile(posY - 2 + c, posX - 2 + r, td));
      //td.addEventListener('click', () => showCoordinates(posX + c, posY + r));

      const p = document.createElement('p');
      p.innerText = '?';
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
    const response = await fetch(`https://localhost:7223/api/Tuiles/GetInitialTuiles/${posX}/${posY}`);
    const initialTiles = await response.json();
    initialTiles.forEach((tile, index) => {
      const row = Math.floor(index / 5);
      const col = index % 5;
      gameGrid[col][row] = tile;
    });
  } catch (error) {
    console.error('Erreur API : ', error);
  }
  await displayGameGrid();
}

// affichage de la grille qui se trouve dans gameGrid
async function displayGameGrid() {
  for(let r = 0; r < 5; r++) {
    for(let c = 0; c < 5; c++) {
      const tile = gameGrid[r][c];
      if(tile) {
        const td = document.getElementById(`tile-${posX - 2 + c}-${posY - 2 + r}`);
        if(td) {
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
        }
      }
    }
  }
}


document.addEventListener('DOMContentLoaded', () => {
  // Gestion du formulaire de connexion
  const loginForm = document.getElementById('login-form');
  if(loginForm){
    loginForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      const email = document.getElementById('email-login').value;
      const password = document.getElementById('password-login').value;
      try {
        const response = await fetch(`https://localhost:7223/api/utilisateurs/login/${email}/${password}`, {
          method: 'POST',
          headers: {'Content-Type': 'application/json'}
        });
        if(response.ok){
          const token = await response.text(); // La réponse renvoie seulement le token JWT
          localStorage.setItem('jwtToken', token);
          updateUIBasedOnAuth();
          document.getElementById('login-message').textContent = 'Connexion réussie !';
        } else {
          const errorText = await response.text();
          document.getElementById('login-message').textContent = 'Erreur: ' + errorText;
        }
      } catch (err){
        document.getElementById('login-message').textContent = 'Erreur de connexion au serveur.';
        console.error('Erreur login:', err);
      }
    });
  }

  // Gestion du formulaire d'inscription
  const registerForm = document.getElementById('register-form');
  if(registerForm){
    registerForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      const email = document.getElementById('email-register').value;
      const pseudo = document.getElementById('pseudo-register').value;
      const password = document.getElementById('password-register').value;
      try {
        const response = await fetch('https://localhost:7223/api/utilisateurs/register', {
          method: 'POST',
          headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({
            email,
            pseudo,
            password
          })
        });
        if(response.ok){
          document.getElementById('register-message').textContent = 'Inscription réussie ! Vous pouvez maintenant vous connecter.';
          // Basculer vers le formulaire de login après inscription réussie
          document.getElementById('register-box').style.display = 'none';
          document.getElementById('login-box').style.display = 'block';
        } else {
          const errorText = await response.text();
          document.getElementById('register-message').textContent = 'Erreur: ' + errorText;
        }
      } catch (err){
        document.getElementById('register-message').textContent = 'Erreur de connexion au serveur.';
        console.error('Erreur register:', err);
      }
    });
  }
});

//GetTuilesLine return tableau de tuile qui represente la ligne up, down, left, right avec la position du centre et non la direction
//GetOrCreateTuile return tuile avec la position X et Y
//GetInitialTuiles retourne la grille initiale 5x5 avec la tuile du centre position du joueur

async function moveGrid(direction) {
  try {
    // Get new tiles line based on direction
    const response = await fetch(`https://localhost:7223/api/Tuiles/GetTuilesLine/${posX}/${posY}/${direction}`);
    const newTiles = await response.json();

    // Update position based on direction
    switch(direction) {
      case 'up':
        posX--;
        break;
      case 'down': 
        posX++;
        break;
      case 'left':
        posY--;
        break;
      case 'right':
        posY++;
        break;
    }

    // Clear existing grid
    gameContainer.innerHTML = '';
    
    // Recreate grid with new position
    await createGrid();

    // Update tiles with new data
    newTiles.forEach(tile => {
      if (!tile) return; // Skip if tile is null
      const td = document.getElementById(`tile-${tile.positionX}-${tile.positionY}`);
      if (td) {
        td.innerHTML = '';
        
        // Only display the new tiles on the appropriate edge based on movement direction
        if (
          (direction === 'up' && tile.positionY === posY) ||
          (direction === 'down' && tile.positionY === posY + 4) ||
          (direction === 'left' && tile.positionX === posX) ||
          (direction === 'right' && tile.positionX === posX + 4)
        ) {
          td.style.cssText = `background-image: url(img/${tile.imageURL})`;
        }
      }
    });

  } catch (error) {
    console.error('Error moving grid:', error);
    showErrorPopup('Failed to move grid');
  }
}


function showErrorPopup(message) {
  const popup = document.createElement('div');
  popup.className = 'error-popup';
  popup.innerHTML = `
  <div class="error-content">
    <p>${message}</p>
    <button onclick="this.parentElement.parentElement.remove()">OK</button>
  </div>
  `;
  document.body.appendChild(popup);

  const style = document.createElement('style');
  document.head.appendChild(style);
}

// Fonction pour vérifier si l'utilisateur est connecté
function isUserLoggedIn() {
  return !!localStorage.getItem('jwtToken');
}

// Fonction pour mettre à jour l'UI selon l'état d'authentification
function updateUIBasedOnAuth() {
  const logoutBtn = document.getElementById('logout-btn');
  
  if (isUserLoggedIn()) {
    // Utilisateur connecté : cacher le formulaire, afficher le jeu et le bouton logout
    document.getElementById('auth-container').style.display = 'none';
    document.getElementById('game-container').style.display = 'flex';
    if (logoutBtn) {
      logoutBtn.style.display = 'block';
    }
  } else {
    // Pas connecté : affichage formulaire, cacher jeu et bouton logout
    document.getElementById('auth-container').style.display = 'block';
    document.getElementById('game-container').style.display = 'none';
    if (logoutBtn) {
      logoutBtn.style.display = 'none';
    }
  }
}

// Gestionnaire de logout
document.getElementById('logout-btn').addEventListener('click', async () => {
  try {
    // Supprimer le token du localStorage
    localStorage.removeItem('jwtToken');
    
    // Mettre à jour l'UI : masquer le jeu, afficher le formulaire de connexion
    updateUIBasedOnAuth();
    
    // Réinitialiser les messages
    document.getElementById('login-message').textContent = '';
    if (document.getElementById('register-message')) {
      document.getElementById('register-message').textContent = '';
    }
    
    console.log('Déconnexion réussie');

  } catch (err) {
    console.error('Erreur lors de la déconnexion:', err);
    alert('Impossible de se déconnecter.');
  }
});

// Gestionnaire pour basculer entre login et register
document.addEventListener('DOMContentLoaded', () => {
  // Vérifier l'état d'auth au chargement
  updateUIBasedOnAuth();
  
  // Toggle vers register
  const showRegisterBtn = document.getElementById('show-register');
  if (showRegisterBtn) {
    showRegisterBtn.addEventListener('click', () => {
      document.getElementById('login-box').style.display = 'none';
      document.getElementById('register-box').style.display = 'block';
    });
  }
  
  // Toggle vers login
  const showLoginBtn = document.getElementById('show-login');
  if (showLoginBtn) {
    showLoginBtn.addEventListener('click', () => {
      document.getElementById('register-box').style.display = 'none';
      document.getElementById('login-box').style.display = 'block';
    });
  }
});
