const gameContainer = document.getElementById('grid-container')
let posX = 8;
let posY = 8;

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
    const response = await fetch(`https://localhost:7223/api/Tuiles/${x}/${y}`);
    const tile = await response.json()
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
  } catch (error) {
    console.error('Erreur API : ', error)
  }
}

function showCoordinates(c, r) { 
  const coord = document.getElementById("coord");
  coord.innerText = '';
  coord.innerHTML = c + ", " + r;
}

async function createGrid() {
  const table = document.createElement('table');
  for (let r = posY; r < posY + 5; r++) {
    const tr = document.createElement('tr');
    for (let c = posX; c < posX + 5; c++) {
      const td = document.createElement('td');

      // Calculer les coordonnées centrées
      // const centeredX = c - Math.floor(cols / 2) + 10;
      // const centeredY = r - Math.floor(rows / 2) + 10;

      // const response = await fetch(`https://localhost:7223/api/Tuiles/GetOrCreateTuile/${c}/${r}`);
      // const tile = await response.json()
      // const centeredX = tile.positionX;
      // const centeredY = tile.positionY;
      
      td.id = `tile-${c}-${r}`;

      //td.addEventListener('mouseover', showCoordinates(c, r));
      td.onclick = () => GetTile(c, r, td);
      const tuileEmpty = document.createElement('p');
      tuileEmpty.style.cssText = `
      Font-Size: 2rem;`
      tuileEmpty.innerText = `?`;
      td.appendChild(tuileEmpty);

      tr.appendChild(td);
    }
    table.appendChild(tr);
  }

  gameContainer.appendChild(table);
  displayDefaultTiles();
}

async function displayDefaultTiles() {
  try {
    const response = await fetch(`https://localhost:7223/api/Tuiles/GetInitialTuile/${posX}/${posY}`);
    const tiles = await response.json();
    
    tiles.forEach(tile => {
      const td = document.getElementById(`tile-${tile.positionX}-${tile.positionY}`);
      if (td) {
        td.innerHTML = '';
        td.style.cssText = `background-image: url(img/${tile.imageURL})`;
      }
    });
  } catch (error) {
    console.error('Erreur lors du chargement des tuiles initiales:', error);
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
//GetInitialTuile retourne la grille initiale 5x5 avec la tuile du centre position du joueur

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

