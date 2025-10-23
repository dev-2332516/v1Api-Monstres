const gameContainer = document.getElementById("grid-container");
let posX = 10;
let posY = 10;

let gameGrid = Array.from({ length: 5 }, () => Array(5).fill(null));

// Map complète
const MAP_SIZE = 50;
let mapArray = Array.from({ length: 49 }, () => Array(49).fill(null));
// let mapArray = Array.from({ length: MAP_SIZE }, () =>
//   Array(MAP_SIZE).fill(null)
// );



let deleteMonstre = false;
let isDefeated = false;
let isIndecis = false;
async function callAPI(route, method, responseType) {
  const dateVar = new Date
  let tokenObject = JSON.parse(localStorage.getItem("jwtToken"));
  const now = Math.floor(new Date().getTime()/1000.0)
  if (!tokenObject || tokenObject.expiry <= now) {
    document.getElementById("logout-btn").click()
  }
  else {
    let token = JSON.parse(tokenObject.value);
    let response = await fetch(`https://localhost:7223/api/${route}`, {
      method: method,
      headers: { userToken: token.token },
    });
    if (responseType == "text") return (result = await response.text());
    if (responseType == "json" || !responseType)
      return (result = await response.json());
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




