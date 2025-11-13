const gameContainer = document.getElementById("grid-container");
let posX = 10;
let posY = 10;

// let gameGrid = Array.from({ length: 5 }, () => Array(5).fill(null));

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

// Système de notifications Toast pour remplacer les anciennes popups
class NotificationSystem {
  constructor() {
    this.notifications = [];
    this.maxNotifications = 5;
    this.defaultDuration = 4000;
    this.container = null;
    this.init();
  }

  init() {
    this.container = document.getElementById('notification-container');
    if (!this.container) {
      this.container = document.createElement('div');
      this.container.id = 'notification-container';
      this.container.className = 'notification-container';
      document.body.appendChild(this.container);
    }
  }

  show(message, type = 'info', options = {}) {
    const {
      title = this.getDefaultTitle(type),
      duration = this.defaultDuration,
      icon = this.getDefaultIcon(type)
    } = options;

    if (this.notifications.length >= this.maxNotifications) {
      this.remove(this.notifications[0]);
    }

    const notification = this.create(message, type, title, icon);
    this.notifications.push(notification);
    this.container.appendChild(notification);

    setTimeout(() => {
      notification.classList.add('show');
    }, 100);

    if (duration > 0) {
      setTimeout(() => {
        this.remove(notification);
      }, duration);
    }

    return notification;
  }

  create(message, type, title, icon) {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    
    notification.innerHTML = `
      <div class="notification-header">
        <div class="notification-title">
          <div class="notification-icon">${icon}</div>
          ${title}
        </div>
        <button class="notification-close" title="Fermer">×</button>
      </div>
      <div class="notification-message">${message}</div>
    `;

    const closeBtn = notification.querySelector('.notification-close');
    closeBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      this.remove(notification);
    });

    notification.addEventListener('click', () => {
      this.remove(notification);
    });

    return notification;
  }

  remove(notification) {
    if (!notification || !notification.parentNode) return;

    notification.classList.add('hide');
    
    setTimeout(() => {
      if (notification.parentNode) {
        notification.parentNode.removeChild(notification);
      }
      const index = this.notifications.indexOf(notification);
      if (index > -1) {
        this.notifications.splice(index, 1);
      }
    }, 400);
  }

  getDefaultTitle(type) {
    const titles = {
      success: '✅ Succès',
      error: '❌ Erreur',
      combat: '⚔️ Combat',
      info: 'ℹ️ Information'
    };
    return titles[type] || 'ℹ️ Notification';
  }

  getDefaultIcon(type) {
    const icons = {
      success: '✓',
      error: '✗',
      combat: '⚔',
      info: 'i'
    };
    return icons[type] || 'i';
  }

  // Méthodes de convenance
  success(message, options = {}) {
    return this.show(message, 'success', options);
  }

  error(message, options = {}) {
    return this.show(message, 'error', { duration: 6000, ...options });
  }

  combat(message, options = {}) {
    return this.show(message, 'combat', { duration: 3000, ...options });
  }

  info(message, options = {}) {
    return this.show(message, 'info', options);
  }
}

// Créer l'instance globale
const notifications = new NotificationSystem();

// Fonction de remplacement pour showErrorPopup (rétrocompatibilité)
function showErrorPopup(message) {
  // Déterminer le type selon le contenu du message
  let type = 'info';
  if (message.toLowerCase().includes('erreur') || message.toLowerCase().includes('error')) {
    type = 'error';
  } else if (message.toLowerCase().includes('tuer') || message.toLowerCase().includes('vaincu')) {
    type = 'combat';
  } else if (message.toLowerCase().includes('succès') || message.toLowerCase().includes('réussi')) {
    type = 'success';
  }
  
  notifications.show(message, type);
}

// Exposer pour utilisation globale
window.notifications = notifications;
window.notify = {
  success: (msg, opts) => notifications.success(msg, opts),
  error: (msg, opts) => notifications.error(msg, opts),
  combat: (msg, opts) => notifications.combat(msg, opts),
  info: (msg, opts) => notifications.info(msg, opts)
};




