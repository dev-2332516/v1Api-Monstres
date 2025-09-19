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
      const username = document.getElementById('login-username').value;
      const password = document.getElementById('login-password').value;
      try {
        const response = await fetch('https://localhost:7223/api/auth/login', {
          method: 'POST',
          headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({username, password})
        });
        if(response.ok){
          //const data = await response.json();
          //localStorage.setItem('token', data.token);
          window.location.href = 'index.html';
        } else {
          alert("Identifiants invalides !");
        }
      } catch (err){
        alert("Erreur de connexion au serveur.");
      }
    });
  }

  // Gestion du formulaire d'inscription
  const registerForm = document.getElementById('register-form');
  if(registerForm){
    registerForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      const email = document.getElementById('register-email').value;
      const username = document.getElementById('register-username').value;
      const password = document.getElementById('register-password').value;
      try {
        const response = await fetch('https://localhost:7223/api/auth/register', {
          method: 'POST',
          headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({
            email,
            username,
            password
          })
        });
        if(response.ok){
          alert("Inscription réussie, connecte-toi !");
          window.location.href = 'login.html';
        } else {
          alert("Erreur lors de l'inscription.");
        }
      } catch (err){
        alert("Erreur de connexion au serveur.");
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
        posY--;
        break;
      case 'down': 
        posY++;
        break;
      case 'left':
        posX--;
        break;
      case 'right':
        posX++;
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

