const gameContainer = document.getElementById('grid-container')

createGrid(5, 5);

displayDefaultTiles();

async function GetTile(x,y, td){
  try{
    const response = await fetch(`https://localhost:7223/api/Tuiles/${x}/${y}`);
    const tile = await response.json()
    td.style.cssText = `
    background-image: url(img/${tile.imageURL})
  `;
  }catch(error){
    console.error('Erreur API : ', error)
  }
}


function createGrid(rows, cols) {
  const table = document.createElement('table');
  for (let r = 0; r < rows; r++) {
    const tr = document.createElement('tr');
    for (let c = 0; c < cols; c++) {
      const td = document.createElement('td');
     
      // Calculer les coordonnées centrées
      const centeredX = c - Math.floor(cols / 2) + 10;
      const centeredY = r - Math.floor(rows / 2) + 10;
      td.id = `tile-${centeredX}-${centeredY}`;

      const button = document.createElement('button');
      button.innerText = `${centeredX},${centeredY}`;
      button.onclick = () => GetTile(centeredX, centeredY, td);
      td.appendChild(button);

      tr.appendChild(td);
    }
    table.appendChild(tr);
  }
  
  gameContainer.appendChild(table);
}

async function displayDefaultTiles() {
  for (let x = -1; x <= 1; x++) {
    for (let y = -1; y <= 1; y++) {
      const td = document.getElementById(`tile-${10-x}-${10-y}`);
      if (td) {
        await GetTile(10-x, 10-y, td);
      }
    }
  }
}

