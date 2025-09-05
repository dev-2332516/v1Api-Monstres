const gameContainer = document.getElementById('')
const grid = [];





async function getTilesAsync(x, y, radius = 10) {
    try {
      const url = `http://localhost:3000/api/world/tiles/area?x=${x}&y=${y}&radius=${radius}`;
      const response = await fetch(url);
      
      if (!response.ok) {
        throw new Error(`Erreur HTTP: ${response.status}`);
      }
      
      const tiles = await response.json();
      displayTiles(tiles);
      
    } catch (error) {
      console.error('Erreur lors du chargement:', error);
      showErrorMessage('Impossible de charger les tuiles');
    }
  }
  