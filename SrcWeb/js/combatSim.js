(function attachCombatSimulator() {
  if (document.getElementById("combat-sim")) return;

  const btn = document.createElement("button");
  btn.id = "combat-sim";
  btn.textContent = "Simulateur de comabt";
  btn.className = "controls";
  btn.addEventListener("click", showCombatSimulator);

  const navContainers = document.querySelectorAll("#settings .container.settings-tuile");
  let inserted = false;
  navContainers.forEach((el) => {
    const title = el.querySelector(".title");
    if (title && title.textContent.trim().toLowerCase() === "navigation") {
      const controls = el.querySelector(".controls") || el;
      controls.appendChild(btn);
      inserted = true;
    }
  });

  if (!inserted) {
    const settingsPanel = document.getElementById("settings") || document.body;
    const wrapper = document.createElement("div");
    wrapper.className = "container settings-tuile";
    const title = document.createElement("p");
    title.className = "title";
    title.textContent = "Outils";
    wrapper.appendChild(title);
    wrapper.appendChild(btn);
    settingsPanel.insertBefore(wrapper, settingsPanel.firstChild);
  }
})();

function showCombatSimulator() {
  if (document.getElementById("combat-modal")) return;

  const modal = document.createElement("div");
  modal.id = "combat-modal";
  modal.className = "container settings-tuile";
  modal.style.position = "fixed";
  modal.style.left = "50%";
  modal.style.top = "50%";
  modal.style.transform = "translate(-50%, -50%)";
  modal.style.zIndex = 9999;
  modal.style.maxWidth = "460px";
  modal.style.width = "90%";
  modal.style.boxSizing = "border-box";
  modal.innerHTML = `
    <p class="title">Combat Simulator</p>

    <div style="display:flex;gap:12px;flex-wrap:wrap;margin-top:8px;">
      <div style="flex:1;min-width:160px;">
        <strong>Player</strong>
        <label>HP <input id="sim-player-hp" type="number" value="50" /></label>
        <label>Attack <input id="sim-player-atk" type="number" value="10" /></label>
        <label>Defense <input id="sim-player-def" type="number" value="2" /></label>
      </div>
      <div style="flex:1;min-width:160px;">
        <strong>Monster</strong>
        <label>HP <input id="sim-mon-hp" type="number" value="30" /></label>
        <label>Attack <input id="sim-mon-atk" type="number" value="8" /></label>
        <label>Defense <input id="sim-mon-def" type="number" value="1" /></label>
      </div>
    </div>

    <div style="display:flex;align-items:center;gap:12px;margin-top:10px;">
      <label style="margin:0;">Runs: <input id="sim-runs" type="number" value="1" min="1" style="width:80px;" /></label>
      <label style="margin:0;"><input id="sim-randomize" type="checkbox" /> ±20% variance</label>
    </div>

    <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:12px;">
      <button id="sim-run-btn">Simulate</button>
      <button id="sim-close-btn">Close</button>
    </div>

    <div id="combat-results" aria-live="polite" style="margin-top:12px;"></div>
  `;

  const gameContainerElem = document.getElementById("game-container") || document.body;
  gameContainerElem.appendChild(modal);

  document.getElementById("sim-close-btn").addEventListener("click", () => modal.remove());

  document.getElementById("sim-run-btn").addEventListener("click", () => {
    const player = {
      hp: Number(document.getElementById("sim-player-hp").value) || 0,
      atk: Number(document.getElementById("sim-player-atk").value) || 0,
      def: Number(document.getElementById("sim-player-def").value) || 0
    };
    const monster = {
      hp: Number(document.getElementById("sim-mon-hp").value) || 0,
      atk: Number(document.getElementById("sim-mon-atk").value) || 0,
      def: Number(document.getElementById("sim-mon-def").value) || 0
    };
    const runs = Math.max(1, Math.floor(Number(document.getElementById("sim-runs").value) || 1));
    const randomize = document.getElementById("sim-randomize").checked;
    const results = runCombatSimulation(player, monster, runs, randomize);
    document.getElementById("combat-results").innerHTML = formatSimResults(results, player, monster, runs);
  });
}

function simulateSingleFight(playerInit, monsterInit, randomize=false) {
  const player = { hp: playerInit.hp, atk: playerInit.atk, def: playerInit.def };
  const monster = { hp: monsterInit.hp, atk: monsterInit.atk, def: monsterInit.def };

  let rounds = 0;
  const VAR = randomize ? 0.2 : 0; // ±20%
  while (player.hp > 0 && monster.hp > 0) {
    rounds++;
    let dmgP = Math.max(1, player.atk - monster.def);
    if (VAR) dmgP = Math.max(1, Math.round(dmgP * (1 + (Math.random()*2 - 1) * VAR)));
    monster.hp -= dmgP;
    if (monster.hp <= 0) return { winner: 'player', rounds, playerHP: player.hp, monsterHP: Math.max(0, monster.hp) };

    let dmgM = Math.max(1, monster.atk - player.def);
    if (VAR) dmgM = Math.max(1, Math.round(dmgM * (1 + (Math.random()*2 - 1) * VAR)));
    player.hp -= dmgM;
    if (player.hp <= 0) return { winner: 'monster', rounds, playerHP: Math.max(0, player.hp), monsterHP: monster.hp };
  }
  return { winner: (player.hp>0) ? 'player' : 'monster', rounds, playerHP: Math.max(0, player.hp), monsterHP: Math.max(0, monster.hp) };
}

function runCombatSimulation(player, monster, runs=1, randomize=false) {
  const stats = { wins: 0, losses: 0, totalRounds: 0, details: [] };
  for (let i = 0; i < runs; i++) {
    const res = simulateSingleFight(player, monster, randomize);
    if (res.winner === 'player') stats.wins++; else stats.losses++;
    stats.totalRounds += res.rounds;
    if (runs <= 50) stats.details.push(res); 
  }
  stats.winRate = (stats.wins / runs) * 100;
  stats.avgRounds = stats.totalRounds / runs;
  return stats;
}

function formatSimResults(stats, player, monster, runs) {
  let out = `<strong>Player</strong>: HP=${player.hp}, ATK=${player.atk}, DEF=${player.def}<br>`;
  out += `<strong>Monster</strong>: HP=${monster.hp}, ATK=${monster.atk}, DEF=${monster.def}<br>`;
  out += `<hr>`;
  out += `<strong>Runs:</strong> ${runs} &nbsp; <strong>Win rate:</strong> ${stats.winRate.toFixed(1)}% &nbsp; <strong>Avg rounds:</strong> ${stats.avgRounds.toFixed(2)}<br>`;
  if (stats.details && stats.details.length) {
    out += `<details><summary>Sample fights (${stats.details.length})</summary><ul>`;
    stats.details.forEach((d, idx) => {
      out += `<li>#${idx+1}: winner=${d.winner}, rounds=${d.rounds}, playerHP=${d.playerHP}, monsterHP=${d.monsterHP}</li>`;
    });
    out += `</ul></details>`;
  }
  out += `<hr>`;
  out += `<strong>Recommendation:</strong> `;
  if (stats.winRate > 75) out += `Haute chance de gagner.`;
  else if (stats.winRate > 50) out += `Léger avantage.`;
  else if (stats.winRate === 50) out += `Indécis.`;
  else out += `Vous allez probablement perdre`;
  return out;
}

window.runCombatSimulation = function(player, monster, runs=1, randomize=false) {
  return runCombatSimulation(player, monster, runs, randomize);
};
