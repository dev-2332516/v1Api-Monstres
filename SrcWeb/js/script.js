const gameContainer = document.getElementById('grid-container')
const posX = 8;
const posY = 8;

createGrid();

// displayDefaultTiles();

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

function showCoordinates() { 
  
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
      const response = await fetch(`https://localhost:7223/api/Tuiles/${c}/${r}`);
      const tile = await response.json()
      const centeredX = tile.positionX;
      const centeredY = tile.positionY;
      
      td.id = `tile-${centeredX}-${centeredY}`;

      td.onclick = () => GetTile(centeredX, centeredY, td);
      td.addEventListener('mouseover', showCoordinates(centeredX, centeredY))
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
  for (let x = -1; x <= 1; x++) {
    for (let y = -1; y <= 1; y++) {
      const td = document.getElementById(`tile-${posX + 2 - x}-${posY + 2 - y}`);
      if (td) {
        await GetTile(posX + 2 - x, posY + 2 - y, td);
      }
    }
  }
}

