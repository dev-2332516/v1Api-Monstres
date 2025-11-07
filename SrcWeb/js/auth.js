const EXPIRY_TIME = 3600

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
    createGrid();
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


document.addEventListener("DOMContentLoaded", () => {
  // Gestionnaire de logout
  const logoutBtnElement = document.getElementById("logout-btn");
  if (logoutBtnElement) {
    logoutBtnElement.addEventListener("click", async () => {
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
      } catch (err) {
        alert("Impossible de se déconnecter.");
      }
    });
  }

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
          const now = new Date()
          const expire = Math.floor(new Date().getTime()/1000.0) + EXPIRY_TIME
          // const tempToken = await response.text(); // La réponse renvoie seulement le token JWT
          const token = {
            value: await response.text(),
            expiry: expire
          }
          localStorage.setItem("jwtToken", JSON.stringify(token));
          updateUIBasedOnAuth();
          document.getElementById("login-message").textContent =
            "Connexion réussie !";
            createGrid();
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